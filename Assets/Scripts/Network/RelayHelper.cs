using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayHelper {
    private IRelayService _RelayService;
    private NetworkManager _Network;
    private UnityTransport _Transport;

    public RelayHelper() {
        _RelayService = RelayService.Instance;
        _Network = NetworkManager.Singleton;
        _Transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public async Task<string> CreateRelay() {
        string joinCode = null;

        try {
            Allocation alloc = await _RelayService.CreateAllocationAsync(1);

            joinCode = await _RelayService.GetJoinCodeAsync(alloc.AllocationId);
    
            Debug.Log("Created relay with join code = " + joinCode);

            _Transport.SetRelayServerData(new(alloc, "dtls"));

            _Network.StartHost();
        } catch (RelayServiceException e) {
            Debug.LogError(e.Message);
        }

        return joinCode;
    }
    
    public async Task JoinRelay(string joinCode) {
        try {
            JoinAllocation alloc = await _RelayService.JoinAllocationAsync(joinCode);

            Debug.Log("Joined relay with join code = " + joinCode);

            _Transport.SetRelayServerData(new(alloc, "dtls"));

            _Network.StartClient();
        } catch (RelayServiceException e) {
            Debug.LogError(e.Message);
        }
    }

    public void Shutdown() {
        _Network.Shutdown();
    }
}
