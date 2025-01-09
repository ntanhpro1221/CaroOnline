using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    public void ClientClicked(Vector3Int pos) {
        MarkHelper.Instance.Mark_O(pos);
        Mark_O_ServerRpc(pos);

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyClientIsWinner_ServerRpc();
        }
    }

    public void HostClicked(Vector3Int pos) {
        MarkHelper.Instance.Mark_X(pos); // for fast
        Mark_X_ClientRpc(pos); // mark to this pos again but no problem

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyHostIsWinner_ClientRpc();
        }
    }

    private void Start() {
        if (!IsOwner) return; 
        if (IsHost) SelectableBoard.Instance.OnCellSelected.AddListener(HostClicked);
        else SelectableBoard.Instance.OnCellSelected.AddListener(ClientClicked);
    }
    
    [ClientRpc]
    public void NotifyHostIsWinner_ClientRpc() {
        BattleConnector.Instance.HandleResult(IsHost); 
    }
    
    [ClientRpc]
    public void NotifyClientIsWinner_ClientRpc() {
        BattleConnector.Instance.HandleResult(!IsHost); 
    }

    [ServerRpc]
    public void NotifyClientIsWinner_ServerRpc()
        => NotifyClientIsWinner_ClientRpc();

    [ClientRpc]
    public void Mark_X_ClientRpc(Vector3Int pos) {
        MarkHelper.Instance.Mark_X(pos); 
    }

    [ClientRpc]
    public void Mark_O_ClientRpc(Vector3Int pos) {
        MarkHelper.Instance.Mark_O(pos);
    }

    [ServerRpc]
    public void Mark_X_ServerRpc(Vector3Int pos) {
        MarkHelper.Instance.Mark_X(pos); 
    }

    [ServerRpc]
    public void Mark_O_ServerRpc(Vector3Int pos) {
        MarkHelper.Instance.Mark_O(pos);
    }
}
