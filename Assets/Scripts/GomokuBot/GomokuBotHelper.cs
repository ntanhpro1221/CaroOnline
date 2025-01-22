using GomokuBot;
using GomokuBot.AI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class GomokuBotHelper {
    private static readonly int searchDeep = 4;
    private static readonly IEvaluate bot = new EvaluateV3();
    private static Vector3Int[] firstMoveDis = new Vector3Int[] {
        new(-1, 1),
        new(1, -1),
        new(1, 1),
        new(-1, -1),
    };

    public static async Task<Vector3Int> GetBotMove(List<(Vector3Int, MarkType)> moveHistory) {
        if (moveHistory.Count == 1) 
            return moveHistory[0].Item1 + firstMoveDis[Random.Range(0, firstMoveDis.Length - 1)];
        return await Task.Run(() => {
            var (offset, size) = GetBoardConvertSize(moveHistory);
            Board board = new(size);
            foreach (var (pos, _) in moveHistory)
                board.PutStoneAndSwitchTurn(new Position(pos.x + offset.x, pos.y + offset.y));

            Minimax minimax = new(board, bot, new StringBuilderLog());
            Position movePos = minimax.calculateNextMove(searchDeep);

            return new Vector3Int(movePos.Row - offset.x, movePos.Col - offset.y);
        });
    }
    
    private static BoardConvertSize GetBoardConvertSize(List<(Vector3Int, MarkType)> moveHistory) {
        if (moveHistory.Count == 0) return new() { size = 1 };

        Vector2Int 
            min = new(int.MaxValue, int.MaxValue), 
            max = new(int.MinValue, int.MinValue);
        
        foreach (var (pos, _) in moveHistory) {
            min.x = Mathf.Min(min.x, pos.x);
            min.y = Mathf.Min(min.y, pos.y);
            max.x = Mathf.Max(max.x, pos.x);
            max.y = Mathf.Max(max.y, pos.y);
        }

        return new() {
            offset = new(
                4 - min.x,
                4 - min.y),
            size = Mathf.Max(max.x - min.x + 10, max.y - min.y + 10),
        };
    }

    private struct BoardConvertSize {
        public Vector2Int offset;
        public int size;

        public void Deconstruct(out Vector2Int offset, out int size) {
            offset = this.offset;
            size = this.size;
        }
    }
}
