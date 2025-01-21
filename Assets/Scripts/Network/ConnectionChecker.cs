using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Lobbies;

public static class ConnectionChecker {
    public static bool CachedInternetCheckResult { get; private set; } = false;

    public static async Task<bool> CheckConnection(int timeout = 10000) {
        Task<bool> requestTask = Task.Run(async () => {
            try {
                if (UnityServices.Instance.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync();
                await LobbyService.Instance.GetJoinedLobbiesAsync();
            } catch (LobbyServiceException e) {
                if (e.Reason == LobbyExceptionReason.NetworkError)
                    return false;
            }
            return true;
        });
        await Task.Delay(timeout);
        return CachedInternetCheckResult = requestTask.IsCompleted && requestTask.Result;
    } 
}