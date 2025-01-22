using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    [NonSerialized] public NetworkVariable<bool> isHandleResultDone = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private BattleConnector Battle => BattleConnector.Instance;

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

    private void Awake() {
        DontDestroyOnLoad(this);
    }

    public void Init() { 
        if (IsOwner) Battle.MyPlayerController = this;
        else Battle.OpponentController = this;

        if (IsOwner) {
            if (IsHost) SelectableBoard.Instance.OnCellSelected.AddListener(HostClicked);
            else SelectableBoard.Instance.OnCellSelected.AddListener(ClientClicked);
        }
    }

    #region START PLAY
    public bool TryBothPlayerReadyToPlay() {
        if (!Battle.ImReadyToPlay || !Battle.OpponentReadyToPlay) return false;
        if (IsHost) BothPlayerReadyToPlay_ClientRpc();
        else BothPlayerReadyToPlay_ServerRpc();
        return true;
    }

    [ClientRpc]
    private void BothPlayerReadyToPlay_ClientRpc() {
        Battle.MakeBetEloBeforeStart().ContinueWith(
            task => Battle.BothReadyToPlay = true);
    }

    [ServerRpc]
    public void BothPlayerReadyToPlay_ServerRpc()
        => BothPlayerReadyToPlay_ClientRpc();
    
    public void ImReadyToPlay() {
        Battle.ImReadyToPlay = true;
        if (TryBothPlayerReadyToPlay()) return;
        if (IsHost) OpponentReadyToPlay_ClientRpc();
        else OpponentReadyToPlay_ServerRpc();
    }
    
    private void OpponentReadyToPlay() {
        Battle.OpponentReadyToPlay = true;
        if (Battle.MyPlayerController.TryBothPlayerReadyToPlay()) return;
    }

    [ClientRpc]
    private void OpponentReadyToPlay_ClientRpc() {
        if (IsHost) return;
        OpponentReadyToPlay();
    }

    [ServerRpc]
    private void OpponentReadyToPlay_ServerRpc() {
        OpponentReadyToPlay();
    }
    #endregion

    #region PLAY AGAIN
    public void PlayAgain() {
        if (IsHost) PlayAgain_ClientRpc();
        else PlayAgain_ServerRpc();
    }

    [ClientRpc]
    public void PlayAgain_ClientRpc() {
        Battle.MakeBetEloBeforeStart().ContinueWith(
            task => Battle.PlayAgain());
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
        Battle.OpponentWantPlayAgain = true;
        if (Battle.IWantPlayAgain) PlayAgain();
    }

    [ServerRpc]
    public void AskForPlayAgain_ServerRpc() {
        Battle.OpponentWantPlayAgain = true;
        if (Battle.IWantPlayAgain) PlayAgain();
    }
    #endregion

    #region NOTIFY WINNER
    [ClientRpc]
    public void NotifyHostIsWinner_ClientRpc(bool showPlayAgain = true) {
        Task _ = Battle.HandleResult(IsHost, showPlayAgain); 
    }

    [ServerRpc]
    public void NotifyHostIsWinner_ServerRpc(bool showPlayAgain = true)
        => NotifyHostIsWinner_ClientRpc(showPlayAgain);

    [ClientRpc]
    public void NotifyClientIsWinner_ClientRpc(bool showPlayAgain = true) {
        Task _ = Battle.HandleResult(!IsHost, showPlayAgain); 
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
