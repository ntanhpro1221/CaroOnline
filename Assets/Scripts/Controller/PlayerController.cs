using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    [NonSerialized] public NetworkVariable<bool> isHandleResultDone = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public void ClientClicked(Vector3Int pos) {
        if (!MarkHelper.Instance.Mark_O(pos)) return;

        Mark_O_ServerRpc(pos);

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyClientIsWinner_ServerRpc();
        }
    }

    public void HostClicked(Vector3Int pos) {
        if (!MarkHelper.Instance.Mark_X(pos)) return;

        Mark_X_ClientRpc(pos, true);

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            NotifyHostIsWinner_ClientRpc();
        }
    }
    
    public void Surrender() {
        if (IsHost) NotifyClientIsWinner_ClientRpc(false);
        else NotifyHostIsWinner_ServerRpc(false);
    }

    private async void Start() {
        if (!IsOwner) BattleConnector.Instance.SetOpponentController(this);
        else BattleConnector.Instance.SetMyController(this);

        await BattleConnector.Instance.WaitForDoneStart();

        if (IsOwner) {
            if (IsHost) SelectableBoard.Instance.OnCellSelected.AddListener(HostClicked);
            else SelectableBoard.Instance.OnCellSelected.AddListener(ClientClicked);
        }
    }

    [ClientRpc]
    public void BothPlayerReadyToPlay_ClientRpc() {
        BattleConnector.Instance.MakeBetEloBeforeStart().ContinueWith(
            task => BattleConnector.Instance.IsStarted = true);
    }

    #region PLAY AGAIN
    public void PlayAgain() {
        if (IsHost) PlayAgain_ClientRpc();
        else PlayAgain_ServerRpc();
    }

    [ClientRpc]
    public void PlayAgain_ClientRpc() {
        BattleConnector.Instance.MakeBetEloBeforeStart().ContinueWith(
            task => BattleConnector.Instance.PlayAgain());
    }

    [ServerRpc]
    public void PlayAgain_ServerRpc()
        => PlayAgain_ClientRpc();

    public void AskForPlayAgain() {
        if (IsHost) AskForPlayAgain_ClientRpc();
        else AskForPlayAgain_ServerRpc();
    }

    [ClientRpc]
    public void AskForPlayAgain_ClientRpc(bool excludeHost = true) {
        if (IsHost && excludeHost) return;
        BattleConnector.Instance.OpponentWantPlayAgain = true;
        if (BattleConnector.Instance.IWantPlayAgain) PlayAgain();
    }

    [ServerRpc]
    public void AskForPlayAgain_ServerRpc() {
        BattleConnector.Instance.OpponentWantPlayAgain = true;
        if (BattleConnector.Instance.IWantPlayAgain) PlayAgain();
    }
    #endregion

    #region NOTIFY WINNER
    [ClientRpc]
    public void NotifyHostIsWinner_ClientRpc(bool showPlayAgain = true) {
        Task _ = BattleConnector.Instance.HandleResult(IsHost, showPlayAgain); 
    }

    [ServerRpc]
    public void NotifyHostIsWinner_ServerRpc(bool showPlayAgain = true)
        => NotifyHostIsWinner_ClientRpc(showPlayAgain);

    [ClientRpc]
    public void NotifyClientIsWinner_ClientRpc(bool showPlayAgain = true) {
        Task _ = BattleConnector.Instance.HandleResult(!IsHost, showPlayAgain); 
    }

    [ServerRpc]
    public void NotifyClientIsWinner_ServerRpc(bool showPlayAgain = true)
        => NotifyClientIsWinner_ClientRpc(showPlayAgain);
    #endregion

    #region MAKE A MOVE
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
    #endregion
}
