﻿using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BattleConnector : SceneSingleton<BattleConnector> {
    private bool _IsPlayerVSPlayer 
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Player;
    private bool _IsPlayerVSBot
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Bot;
    private NetworkManager _Network 
        => NetworkManager.Singleton;

    private async void Start() {
        if (_IsPlayerVSPlayer) {
            // load opponent infomation before start connect
            OpponentIdFirebase = _Network.IsHost
                ? await DataHelper.UnityToFirebase(LobbyHelper.Instance.ClientUnityId)
                : await DataHelper.UnityToFirebase(LobbyHelper.Instance.HostUnityId);
            OpponentElo = (await DataHelper.LoadUserDataAsync(OpponentIdFirebase)).elo;
            
            // init controller
            foreach (var controller in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                controller.Init();

            // when client disconnect handle
            _Network.OnClientDisconnectCallback += OnClientDisconnectedCallback;

            MyPlayerController.ImReadyToPlay();
        } else if (_IsPlayerVSBot) {
            Instantiate(_PlayerBotController);
        }
    }

    private void OnDisable() {
        _IsQuited = true;
        if (_IsPlayerVSPlayer) {
            Destroy(MyPlayerController.gameObject);
            Destroy(OpponentController.gameObject);
            _Network.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
            _Network.Shutdown();
        }
    }
   
    public async Task HandleResult(bool win, bool showPlayAgain = true, string customTitle = null) {
        _IsPlaying = false;

        SoundHelper.Play(win ? SoundType.Victory : SoundType.Lose);

        if (_IsPlayerVSPlayer) {
            IWantPlayAgain = OpponentWantPlayAgain = false;
            int delPoint = await HandlePoint(win);

            HandleResultPopupPvP(win, delPoint, showPlayAgain, customTitle);

            MyPlayerController.isHandleResultDone.Value = true;
        } else if (_IsPlayerVSBot) {
            HandleResultPopupPlayerVSBot(win);  
        }
    }
    
    public void Exit() {
        if (_IsPlayerVSPlayer) _Network.Shutdown();
        LoadSceneHelper.LoadScene("LobbyScene");
    }

    #region Player vs Bot STUFF
    [SerializeField] private GameObject _PlayerBotController;

    public void HandleResultPopupPlayerVSBot(bool win) {
        PopupFactory.ShowPopup_YesNo(
            win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA",
            null,
            new() {
                content = "Về sảnh chính",
                callback = Exit,
                backgroundColor = Color.red,
                foregroundColor = Color.yellow,
            },
            new() {
                content = "Làm lại",
                callback = async () => {
                    MarkHelper.Instance.ResetForNewGame();
                    WinLineColor.Instance.ClearColor();
                    await PlayerBotController.Instance.BeginTurn();
                },
                backgroundColor = Color.green,
            }
        ).WithExitable(false);
    }
    #endregion

    #region PvP STUFF
    public bool ImReadyToPlay = false;
    public bool OpponentReadyToPlay = false;
    public bool BothReadyToPlay = false;
    
    public bool IWantPlayAgain = false;
    public bool OpponentWantPlayAgain = false;

    private bool _IsQuited = false;
    private bool _IsPlaying = false;

    public string OpponentIdFirebase { get; private set; }
    public int OpponentElo { get; private set; }

    private int BetElo;
    private BasePopup _ResultPopup;

    public PlayerController MyPlayerController;
    public PlayerController OpponentController;
    
    public async Task MakeBetEloBeforeStart() {
        _IsPlaying = true;
        BetElo = -EloDeltaEvaluate.GetDeltaElo(DataHelper.UserData.elo, OpponentElo, false);
        DataHelper.UserData.elo -= BetElo;
        await DataHelper.SaveCurrentUserDataAsync();
    }

    private async void OnClientDisconnectedCallback(ulong id) {
        if (!_IsPlaying) return;
        await Task.Delay(5000);
        if (_IsQuited) return;
        await HandleResult(true, false, "Bạn đã thắng do đối thủ của bạn đã thoát");
    }

    public async Task WaitForDoneStart() {
        while (!BothReadyToPlay) await Task.Delay(33);
    }
    
    public async Task WaitForOpponentHandleResult() {
        while (!OpponentController.isHandleResultDone.Value) await Task.Delay(33);
    }
    
    public async Task WaitForMeHandleResult() {
        while (!MyPlayerController.isHandleResultDone.Value) await Task.Delay(33);
    }

    private async Task<int> HandlePoint(bool win) {
        // restore may elo
        DataHelper.UserData.elo += BetElo;

        // Calc for opponent
        int delPoint_Oppo = EloDeltaEvaluate.GetDeltaElo(
            OpponentElo,
            DataHelper.UserData.elo,
            !win);
        
        // Calc for me
        int delPoint = EloDeltaEvaluate.GetDeltaElo(
            DataHelper.UserData.elo,
            OpponentElo,
            win);
        
        DataHelper.UserData.elo += delPoint;
        OpponentElo += delPoint_Oppo;

        await DataHelper.SaveCurrentUserDataAsync();

        return delPoint;
    }
    
    public void PlayAgain() {
        if (!IWantPlayAgain) return;
        IWantPlayAgain = OpponentWantPlayAgain = false;
        WinLineColor.Instance.ClearColor();
        MarkHelper.Instance.ResetForNewGame();
        _ResultPopup.Disappear();
    }

    private void HandleResultPopupPvP(bool win, int delPoint, bool showPlayAgain = true, string customTitle = null) {
        ButtonField.CreateOption exitBtn = new() {
            content = "Về sảnh chính",
            callback = Exit,
            backgroundColor = Color.red,
            foregroundColor = Color.yellow,
        };
        _ResultPopup = PopupFactory.ShowPopup_ManualBuild();
        _ResultPopup
            .WithTitle(customTitle ?? (win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA"))
            .WithContent(win ? $"+{delPoint} elo!" : $"{delPoint} elo!")
            .WithButton(exitBtn)
            .WithContentColor(win ? Color.green : Color.red)
            .WithExitable(false);

        if (NetworkManager.Singleton.ConnectedClients.Count > 1 && showPlayAgain)
            _ResultPopup.WithButton(new() {
                content = "Làm lại",
                callback = () => {
                    if (NetworkManager.Singleton.ConnectedClients.Count < 2) {
                        PopupFactory.ShowSimplePopup("Đối thủ của bạn thoát rồi :v");
                        return;
                    }
                    IWantPlayAgain = true;
                    _ResultPopup
                        .WithTitle($"Đang đợi đối thủ chấp nhận")
                        .WithContent(null)
                        .WithLoadingIcon(true)
                        .WithoutButton()
                        .WithButton(exitBtn);
                    Debug.Log("OpponentWantPlayAgain: " + OpponentWantPlayAgain);
                    if (OpponentWantPlayAgain) MyPlayerController.PlayAgain();
                    else MyPlayerController.AskForPlayAgain();
                },
                backgroundColor = Color.green, });
    }

    public void Surrender() 
        => MyPlayerController.Surrender();
    #endregion
}
