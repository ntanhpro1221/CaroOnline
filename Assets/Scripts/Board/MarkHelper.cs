using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[DefaultExecutionOrder(-50)]
public class MarkHelper : SceneSingleton<MarkHelper> {
    private bool _BeginIsXTurn = true;
    public bool IsXTurn { get; private set; } = true;
    public List<(Vector3Int, MarkType)> MoveHistory { get; private set; } = new();

    [SerializeField] private TileBase _Mark_O;
    [SerializeField] private TileBase _Mark_X;

    private Tilemap _Map => GetComponent<Tilemap>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>Is this move valid</returns>
    public bool Mark_O(Vector3Int pos) {
        // Check empty
        if (_Map.HasTile(pos)) return false;

        // Check true turn
        if (IsXTurn) {
            VibrateHelper.Vibrate();
            PopupFactory.ShowSimplePopup("Not your turn!");
            return false;
        }

        MoveHistory.Add((pos, MarkType.O));
        SoundHelper.Play(SoundType.MakeMove);
        _Map.SetTile(pos, _Mark_O);
        IsXTurn = !IsXTurn;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>Is this move valid</returns>
    public bool Mark_X(Vector3Int pos) {
        // Check empty
        if (_Map.HasTile(pos)) return false;

        // Check true turn
        if (!IsXTurn) {
            VibrateHelper.Vibrate();
            PopupFactory.ShowSimplePopup("Not your turn!");
            return false;
        }

        MoveHistory.Add((pos, MarkType.X));
        SoundHelper.Play(SoundType.MakeMove);
        _Map.SetTile(pos, _Mark_X);
        IsXTurn = !IsXTurn;
        return true;
    }
    
    public void ResetForNewGame() {
        IsXTurn = _BeginIsXTurn = !_BeginIsXTurn;
        _Map.ClearAllTiles();
        MoveHistory.Clear();
    }

    public void Unmark(Vector3Int pos) 
        => _Map.SetTile(pos, null);

    public void HasMark(Vector3Int pos) 
        => _Map.HasTile(pos);
    
    public bool IsThisMoveMakeWin(Vector3Int pos, bool colorWinLine = true) {
        if (!_Map.HasTile(pos)) return false;
        if (LonggestConsecutiveMatch_FromThisMark(pos).Count >= 5) {
            WinLineColor.Instance.ColorWinLine(pos, _Map.GetTile(pos) == _Mark_X ? MarkType.X : MarkType.O);
            return true;
        }
        return false;
    }

    public List<Vector3Int> LonggestConsecutiveMatch_FromThisMark(Vector3Int pos) {
        List<Vector3Int> longgest = new();

        Vector3Int[] dels = new Vector3Int[] {
            new(0, 1),
            new(1, 0),
            new(1, 1),
            new(1, -1) };
        TileBase pattern = _Map.GetTile(pos);

        foreach (var del in dels) {
            List<(TileBase, Vector3Int)> tileList = new();
            Vector3Int itePos = pos - 4 * del;
            for (int i = 4 * 2 + 1; i > 0; --i) {
                tileList.Add((_Map.GetTile(itePos), itePos));
                itePos += del;
            }
            var curList = LonggestConsecutiveMatch(pattern, tileList);
            if (curList.Count > longgest.Count)
                longgest = new(curList);
        }

        return longgest;
    }

    private List<Vector3Int> LonggestConsecutiveMatch(TileBase pattern, List<(TileBase, Vector3Int)> tileList) {
        List<Vector3Int> longgest = new();
        List<Vector3Int> curSeq = new();

        foreach (var (tile, pos) in tileList) {
            if (tile == pattern) curSeq.Add(pos);
            else curSeq.Clear();

            if (curSeq.Count > longgest.Count)
                longgest = new(curSeq);
        }

        return longgest;
    }
}