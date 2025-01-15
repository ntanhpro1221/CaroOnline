using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleConnector : SceneSingleton<BattleConnector> {
    public bool IsStarted { get; private set; } = false;

    private void Start() {
        NetworkManager net = NetworkManager.Singleton;
        net.OnClientConnectedCallback += OnClientConnectedCallback;
        if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
            net.StartHost();
        } else {
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
        while (!IsStarted) await Task.Delay(200);
    }

    public void HandleResult(bool win) {
        PopupFactory.Instance.ShowPopup_YesNo(
            win ? "BẠN ĐÃ THẮNG" : "BẠN ĐÃ THUA",
            win ? "+500 điểm danh vọng!" : "-500 điểm danh vọng",
            new() {
                content = "Về sảnh chính",
                callback = () => {
                    LobbyHelper.Instance.RelayHelper.Shutdown();
                    SceneManager.LoadScene("LobbyScene");
                },
                backgroundColor = Color.red,
            },
            new() {
                content = "Làm lại",
                callback = () => {
                    print("Gạ gạ gạ");
                },
                backgroundColor = Color.green,
            }
        );
    }

    private void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }
}
