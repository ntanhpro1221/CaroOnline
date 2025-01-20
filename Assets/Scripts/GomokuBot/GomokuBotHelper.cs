using GomokuBot;
using GomokuBot.AI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class GomokuBotHelper {
    private static readonly int searchDeep = 4;
    private static readonly IEvaluate bot = new EvaluateV3();

    public static async Task<Vector3Int> GetBotMove(List<(Vector3Int, MarkType)> moveHistory) {
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
            min.x = Math.Min(min.x, pos.x);
            min.y = Math.Min(min.y, pos.y);
            max.x = Math.Max(max.x, pos.x);
            max.y = Math.Max(max.y, pos.y);
        }

        return new() {
            offset = new(
                4 - min.x,
                4 - min.y),
            size = Math.Max(max.x - min.x + 10, max.y - min.y + 10),
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
