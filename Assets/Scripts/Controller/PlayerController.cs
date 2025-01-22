using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    [NonSerialized] public NetworkVariable<bool> isHandleResultDone = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    [NonSerialized] public NetworkVariable<bool> isReadyToPlay = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    [NonSerialized] public NetworkVariable<bool> isWantPlayAgain = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private BattleConnector Battle 
        => BattleConnector.Instance;

    private void Awake() {
        DontDestroyOnLoad(this);
    }

    public void Init() { 
        if (IsOwner) Battle.MyPlayerController = this;
        else Battle.OpponentController = this;

        if (IsOwner) {
            if (IsHost) SelectableBoard.Instance.OnCellSelected.AddListener(MakeMove);
            else SelectableBoard.Instance.OnCellSelected.AddListener(MakeMove);
        }
    }

    #region MAKE A MOVE
    private void MakeMove(Vector3Int pos) {
        Debug.LogWarning("make move được gọi????????");
        if (!MarkHelper.Instance.MakeMove(pos, IsHost == IsOwner)) return;

        if (IsOwner) {
            if (IsHost) MakeMove_OtherSide_ClientRpc(pos);
            else MakeMove_OtherSide_ServerRpc(pos);
        }

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) NotifyWinner(IsOwner);
    }

    [ClientRpc]
    private void MakeMove_OtherSide_ClientRpc(Vector3Int pos) {
        if (IsHost) return;
        MakeMove(pos);
    }

    [ServerRpc]
    private void MakeMove_OtherSide_ServerRpc(Vector3Int pos) {
        MakeMove(pos);
    }
    #endregion

    private async void NotifyWinner(bool win, bool showPlayAgain = true, string customTitle = null)
        => await Battle.HandleResult(win, showPlayAgain, customTitle);

    #region SURRENDER
    public void Surrender() {
        if (!IsOwner) throw new Exception("Only the owner can invoke this function");
        if (IsHost) Surrender_OtherSide_ClientRpc();
        else Surrender_OtherSide_ServerRpc();
    }

    [ClientRpc]
    private void Surrender_OtherSide_ClientRpc() {
        if (IsHost) return;
        Surrender_OtherSide();
    }

    [ServerRpc]
    private void Surrender_OtherSide_ServerRpc()
        => Surrender_OtherSide();

    private void Surrender_OtherSide()
        => NotifyWinner(true, false, "Bạn đã thắng do đối thử của bạn đã thoát trận");
    #endregion
}
