using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class BattleConnector : SceneSingleton<BattleConnector> {
    private bool _IsPlayerVSPlayer 
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Player;
    private bool _IsPlayerVSBot
        => DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Bot;

    private async void Start() {
        if (_IsPlayerVSPlayer) {
            NetworkManager net = NetworkManager.Singleton;
            if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
                OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetClientUnityId());
                net.OnClientConnectedCallback += OnClientConnectedCallback;
                net.StartHost();
            } else {
                OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetHostUnityId());
                net.StartClient();
            }
        } else if (_IsPlayerVSBot) {
            Instantiate(_PlayerBotController);
        }
    }

    private void OnDisable() {
        if (_IsPlayerVSPlayer && AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    public async Task HandleResult(bool win, bool showPlayAgain = true) {
        SoundHelper.Play(win ? SoundType.Victory : SoundType.Lose);

        if (_IsPlayerVSPlayer) {
            IWantPlayAgain = OpponentWantPlayAgain = false;
            int delPoint = await HandlePoint(win);

            HandleResultPopupPvP(win, delPoint, showPlayAgain);

            MyPlayerController.isHandleResultDone.Value = true;
        } else if (_IsPlayerVSBot) {
            HandleResultPopupPlayerVSBot(win);  
        }
    }
    
    public void Exit() {
        if (_IsPlayerVSPlayer) {
            LobbyHelper.Instance.RelayHelper.Shutdown();
        }
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
                    MarkHelper.Instance.ClearAllMark();
                    await PlayerBotController.Instance.BeginTurn();
                },
                backgroundColor = Color.green,
            }
        ).WithExitable(false);
    }
    #endregion

    #region PvP STUFF
    public bool IsStarted { get; set; } = false;

    public string OpponentIdFirebase { get; private set; }

    public bool IWantPlayAgain = false;
    public bool OpponentWantPlayAgain = false;

    private BasePopup _ResultPopup;

    private PlayerController MyPlayerController;
    private PlayerController OpponentController;

    public void SetMyController(PlayerController controller)
        => MyPlayerController = controller;

    public void SetOpponentController(PlayerController controller) {
        OpponentController = controller;
    }

    private async void OnClientConnectedCallback(ulong id) {
        if (MyPlayerController != null && OpponentController != null) {
            MyPlayerController.BothPlayerReadyToPlay_ClientRpc();
            await LobbyHelper.Instance.DeleteHostedLobby();
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    public async Task WaitForDoneStart() {
        while (!IsStarted) await Task.Delay(100);
    }
    
    public async Task WaitForOpponentHandleResult() {
        while (!OpponentController.isHandleResultDone.Value) await Task.Delay(100);
    }
    
    public async Task WaitForMeHandleResult() {
        while (!MyPlayerController.isHandleResultDone.Value) await Task.Delay(100);
    }

    private async Task<int> HandlePoint(bool win) {
        int delPoint = EloDeltaEvaluate.GetDeltaElo(
            DataHelper.UserData.elo,
            (await DataHelper.LoadUserDataAsync(OpponentIdFirebase)).elo,
            win);

        DataHelper.UserData.elo += delPoint;
        await DataHelper.SaveCurrentUserDataAsync();

        return delPoint;
    }
    
    public void PlayAgain() {
        if (!IWantPlayAgain) return;
        IWantPlayAgain = OpponentWantPlayAgain = false;
        MarkHelper.Instance.ClearAllMark();
        _ResultPopup.Disappear();
    }

    private void HandleResultPopupPvP(bool win, int delPoint, bool showPlayAgain = true) {
        ButtonField.CreateOption exitBtn = new() {
            content = "Về sảnh chính",
            callback = Exit,
            backgroundColor = Color.red,
            foregroundColor = Color.yellow,
        };
        _ResultPopup = PopupFactory.ShowPopup_ManualBuild();
        _ResultPopup
            .WithTitle(win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA")
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
