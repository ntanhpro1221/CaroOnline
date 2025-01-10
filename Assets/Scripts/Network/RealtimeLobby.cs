using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Threading;

public class RealtimeLobby {
    public Lobby Value { get; private set; }

    private readonly ILobbyService _LobbyService;
    private CancellableTask _Task_DoHeartbeat;
    private CancellableTask _Task_SyncData;
    
    public RealtimeLobby(ILobbyService lobbyService) {
        _LobbyService = lobbyService;
    }

    private async Task DoHeartbeat(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) await Task.WhenAll(
            Task.Delay(LobbyHelper.RATE_HEARTBEAT),
            _LobbyService.SendHeartbeatPingAsync(Value.Id));
    }

    private async Task SyncData(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) await Task.WhenAll(
            Task.Delay(LobbyHelper.RATE_GET),
            _LobbyService.GetLobbyAsync(Value.Id).ContinueWith(task => Value = task.Result));
    }
    
    public void StartSync(Lobby lobby, bool isHost) {
        StopSync();
        Value = lobby;
        if (isHost) _Task_DoHeartbeat = new(DoHeartbeat);
        _Task_SyncData = new(SyncData);
    }

    public void StopSync() {
        _Task_DoHeartbeat?.Cancel(); 
        _Task_DoHeartbeat = null;
        _Task_SyncData?.Cancel(); 
        _Task_SyncData = null;
    }
}
