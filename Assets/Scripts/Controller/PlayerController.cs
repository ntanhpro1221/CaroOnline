using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    [NonSerialized] public NetworkVariable<bool> isHandleResultDone = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public void ClientClicked(Vector3Int pos) {
        Debug.Log("Client click");
        if (!MarkHelper.Instance.Mark_O(pos)) return;

        Mark_O_ServerRpc(pos);

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyClientIsWinner_ServerRpc();
        }
    }

    public void HostClicked(Vector3Int pos) {
        Debug.Log("Host click");
        if (!MarkHelper.Instance.Mark_X(pos)) return;

        Mark_X_ClientRpc(pos, true);

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyHostIsWinner_ClientRpc();
        }
    }
    
    public void Surrender() {
        if (IsHost) NotifyClientIsWinner_ClientRpc();
        else NotifyHostIsWinner_ServerRpc();
    }

    private async void Start() {
        await BattleConnector.Instance.WaitForDoneStart();
        if (!IsOwner) {
            BattleConnector.Instance.SetOpponentController(this);
        } else {
            BattleConnector.Instance.SetMyController(this);
            if (IsHost) SelectableBoard.Instance.OnCellSelected.AddListener(HostClicked);
            else SelectableBoard.Instance.OnCellSelected.AddListener(ClientClicked);
        }
    }
    
    [ClientRpc]
    public void NotifyHostIsWinner_ClientRpc() {
        Task _ = BattleConnector.Instance.HandleResult(IsHost); 
    }

    [ServerRpc]
    public void NotifyHostIsWinner_ServerRpc()
        => NotifyHostIsWinner_ClientRpc();

    [ClientRpc]
    public void NotifyClientIsWinner_ClientRpc() {
        Task _ = BattleConnector.Instance.HandleResult(!IsHost); 
    }

    [ServerRpc]
    public void NotifyClientIsWinner_ServerRpc()
        => NotifyClientIsWinner_ClientRpc();

    [ClientRpc]
    public void Mark_X_ClientRpc(Vector3Int pos, bool excludeHost = false) { 
        if (excludeHost && IsHost) return;
        MarkHelper.Instance.Mark_X(pos);
    }

    [ClientRpc]
    public void Mark_O_ClientRpc(Vector3Int pos, bool excludeHost = false) { 
        if (excludeHost && IsHost) return;
        MarkHelper.Instance.Mark_O(pos);
    }

    [ServerRpc]
    public void Mark_X_ServerRpc(Vector3Int pos)
        => MarkHelper.Instance.Mark_X(pos); 

    [ServerRpc]
    public void Mark_O_ServerRpc(Vector3Int pos)
        => MarkHelper.Instance.Mark_O(pos);
}
