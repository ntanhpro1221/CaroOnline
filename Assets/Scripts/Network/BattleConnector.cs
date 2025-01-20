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
            net.OnClientConnectedCallback += OnClientConnectedCallback;
            if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
                OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetClientUnityId());
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
        if (_IsPlayerVSPlayer) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    public async Task HandleResult(bool win) {
        SoundHelper.Play(win ? SoundType.Victory : SoundType.Lose);

        if (_IsPlayerVSPlayer) {
            int delPoint = await HandlePoint(win);

            HandleResultPopupPvP(win, delPoint);

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
                callback = () => { print("Gạ gạ gạ"); },
                backgroundColor = Color.green,
            }
        ).WithExitable(false);
    }
    #endregion

    #region PvP STUFF
    public bool IsStarted { get; private set; } = false;

    public string OpponentIdFirebase { get; private set; }

    private PlayerController MyPlayerController;
    private PlayerController OpponentController;

    public void SetMyController(PlayerController controller)
        => MyPlayerController = controller;

    public void SetOpponentController(PlayerController controller) {
        OpponentController = controller;
    }

    private async void OnClientConnectedCallback(ulong id) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.ConnectedClients.Count == 2) {
            IsStarted = true;
            if (net.IsHost) {
                await LobbyHelper.Instance.DeleteHostedLobby();
            }
            net.OnClientConnectedCallback -= OnClientConnectedCallback;
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
    
    private void HandleResultPopupPvP(bool win, int delPoint) {
        PopupFactory.ShowPopup_YesNo(
            win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA",
            win ? $"+{delPoint} điểm danh vọng!" : $"{delPoint} điểm danh vọng",
            new() {
                content = "Về sảnh chính",
                callback = Exit,
                backgroundColor = Color.red,
                foregroundColor = Color.yellow,
            },
            new() {
                content = "Làm lại",
                callback = () => { print("Gạ gạ gạ"); },
                backgroundColor = Color.green,
            }
        ).WithContentColor(win ? Color.green : Color.red)
        .WithExitable(false);
    }

    public void Surrender() 
        => MyPlayerController.Surrender();
    #endregion
}
