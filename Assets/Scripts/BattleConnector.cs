using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;

public class BattleConnector : SceneSingleton<BattleConnector> {
    public bool IsStarted { get; private set; } = false;

    private void Start() {
        NetworkManager net = NetworkManager.Singleton;
        net.OnClientConnectedCallback += (id) => {
            print($"Connect có {net.ConnectedClients.Count} người nè");
            if (net.ConnectedClients.Count == 2) IsStarted = true;
        };
        if (AuthenticationService.Instance.PlayerId == LobbyHelper.Instance.JoinedLobby.Value.HostId) {
            net.StartHost();
        } else {
            net.StartClient();
        }
    }

    public async Task WaitForDoneStart() {
        while (!IsStarted) await Task.Delay(200);
    }

    public void HandleResult(bool win) {
        if (win) print("+++++++YOU WIN++++++");
        else print("++++++YOU LOSE++++++");
    }
}
