using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Threading;

public class AutoBeatLobby {
    public Lobby Value { get; private set; }

    private readonly ILobbyService _LobbyService;
    private CancellableTask _Task_DoHeartbeat;
    
    public AutoBeatLobby(ILobbyService lobbyService) {
        _LobbyService = lobbyService;
    }

    private async Task DoHeartbeat(CancellationToken tokken) {
        while (!tokken.IsCancellationRequested) await Task.WhenAll(
            Task.Delay(LobbyHelper.RATE_HEARTBEAT),
            _LobbyService.SendHeartbeatPingAsync(Value.Id));
    }

    public void StartSync(Lobby lobby, bool isHost) {
        StopSync();
        Value = lobby;
        if (isHost) _Task_DoHeartbeat = new(DoHeartbeat);
    }

    public void StopSync() {
        _Task_DoHeartbeat?.Cancel(); 
        _Task_DoHeartbeat = null;
        Value = null;
    }
}
