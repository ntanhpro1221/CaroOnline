using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class BattleConnector : SceneSingleton<BattleConnector> {
    public bool IsStarted { get; private set; } = false;

    public string OpponentIdFirebase { get; private set; }

    private PlayerController MyPlayerController;
    private PlayerController OpponentController;

    public void SetMyController(PlayerController controller)
        => MyPlayerController = controller;

    public void SetOpponentController(PlayerController controller) {
        OpponentController = controller;
    }

    private async void Start() {
        NetworkManager net = NetworkManager.Singleton;
        net.OnClientConnectedCallback += OnClientConnectedCallback;
        if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
            OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetClientUnityId());
            net.StartHost();
        } else {
            OpponentIdFirebase = await DataHelper.UnityToFirebase(LobbyHelper.Instance.GetHostUnityId());
            net.StartClient();
        }
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
    
    private void HandleResultPopup(bool win, int delPoint) {
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

    public async Task HandleResult(bool win) {
        SoundHelper.Play(win ? SoundType.Victory : SoundType.Lose);

        int delPoint = await HandlePoint(win);

        HandleResultPopup(win, delPoint);

        MyPlayerController.isHandleResultDone.Value = true;
    }
    
    public void Surrender() 
        => MyPlayerController.Surrender();

    public void Exit() {
        LobbyHelper.Instance.RelayHelper.Shutdown();
        LoadSceneHelper.LoadScene("LobbyScene");
    }

    private void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }
}
