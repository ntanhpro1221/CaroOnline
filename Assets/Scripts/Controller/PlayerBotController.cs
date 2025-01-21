using System.Threading.Tasks;
using UnityEngine;

public class PlayerBotController : SceneSingleton<PlayerBotController> {
    private async void PlayerClicked(Vector3Int pos) {
        if (!MarkHelper.Instance.Mark_O(pos)) return;

        if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
            await BattleConnector.Instance.HandleResult(true);
            return;
        }

        await BotMove();
    }

    private async Task BotMove() {
        async void BotClicked(Vector3Int pos) {
            if (!MarkHelper.Instance.Mark_X(pos)) return;

            if (MarkHelper.Instance.IsThisMoveMakeWin(pos)) {
                await BattleConnector.Instance.HandleResult(false);
                return;
            }
        }
        BotClicked(await GomokuBotHelper.GetBotMove(MarkHelper.Instance.MoveHistory));
    }

    private async void Start() {
        SelectableBoard.Instance.OnCellSelected.AddListener(PlayerClicked);
        await BeginTurn();
    }

    public async Task BeginTurn() {
        if (MarkHelper.Instance.IsXTurn)
            await BotMove();
    }
}
