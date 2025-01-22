using System.Threading.Tasks;
using UnityEngine;

public class PlayerBotController : SceneSingleton<PlayerBotController> {
    private async void MakeMove(Vector3Int pos, bool isPlayer) {
        if (!MarkHelper.Instance.MakeMove(pos, isPlayer)) return;

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            await BattleConnector.Instance.HandleResult(isPlayer);
            return;
        } 
        
        if (isPlayer) await BotMove();
    }

    private async Task BotMove()
        => MakeMove(await GomokuBotHelper.GetBotMove(MarkHelper.Instance.MoveHistory), false);

    private async void Start() {
        SelectableBoard.Instance.OnCellSelected.AddListener(pos => MakeMove(pos, true));
        await BeginTurn();
    }

    public async Task BeginTurn() {
        if (!MarkHelper.Instance._IsHostOrPlayerBegin)
            await BotMove();
    }
}
