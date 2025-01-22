using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[DefaultExecutionOrder(-50)]
public class MarkHelper : SceneSingleton<MarkHelper> {
    public bool _IsHostOrPlayerBegin = true;
    private bool _IsCurTurnIsHostOrPlayer = true;
    public MarkType CurTurnMark { get; private set; } = MarkType.X;
    public List<(Vector3Int, MarkType)> MoveHistory { get; private set; } = new();

    [Header("Mark tile")]
    [SerializeField] private TileBase _Mark_O;
    [SerializeField] private TileBase _Mark_X;
    [Space]
    [Header("Color last mark")]
    [SerializeField] private TileBase _LastColorCell;
    [SerializeField] private Color _LastMarkColor_O;
    [SerializeField] private Color _LastMarkColor_X;
    [SerializeField] private Tilemap _LastMarkMap;

    private Tilemap _Map => GetComponent<Tilemap>();

    private void ColorLastMark(Vector3Int pos, bool isHostOrPlayer) {
        _LastMarkMap.ClearAllTiles();

        switch (DataHelper.SceneBoostData.battle.battleMode) {
            case BattleMode.Player_Player:
                if (isHostOrPlayer == NetworkManager.Singleton.IsHost) return;
                break;
            case BattleMode.Player_Bot:
                if (isHostOrPlayer) return;
                break;
        }

        _LastMarkMap.SetTile(pos, _LastColorCell);
        _LastMarkMap.SetColor(pos, 
            CurTurnMark == MarkType.X
            ? _LastMarkColor_X 
            : _LastMarkColor_O);
    }

    private TileBase TileOfMark(MarkType type) =>
        type == MarkType.X
        ? _Mark_X
        : _Mark_O;

    private void SwitchTurn() {
        CurTurnMark =
            CurTurnMark == MarkType.X
            ? MarkType.O
            : MarkType.X;
        _IsCurTurnIsHostOrPlayer = !_IsCurTurnIsHostOrPlayer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>Is this move valid</returns>
    public bool MakeMove(Vector3Int pos, bool isHostOrPlayer) {
        // Check empty
        if (_Map.HasTile(pos)) return false;

        // Check true turn
        if (_IsCurTurnIsHostOrPlayer != isHostOrPlayer) {
            VibrateHelper.Vibrate();
            PopupFactory.ShowSimplePopup("Not your turn!");
            return false;
        }
        
        if (DataHelper.SceneBoostData.battle.battleMode == BattleMode.Player_Player &&
            MoveHistory.Count == 0 &&
            _IsHostOrPlayerBegin != NetworkManager.Singleton.IsHost) {
            BattleScreenController.Instance.FocusOn(_Map.CellToWorld(pos));
        }

        ColorLastMark(pos, isHostOrPlayer);
        MoveHistory.Add((pos, CurTurnMark));
        SoundHelper.Play(SoundType.MakeMove);
        _Map.SetTile(pos, TileOfMark(CurTurnMark));

        SwitchTurn();
        return true;
    }

    public void ResetForNewGame() {
        CurTurnMark = MarkType.X;
        _IsCurTurnIsHostOrPlayer = _IsHostOrPlayerBegin = !_IsHostOrPlayerBegin;
        _Map.ClearAllTiles();
        _LastMarkMap.ClearAllTiles();
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