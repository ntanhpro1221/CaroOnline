using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MarkHelper : SceneSingleton<MarkHelper> {
    [SerializeField] private TileBase _Mark_O;
    [SerializeField] private TileBase _Mark_X;

    private Tilemap _Map;

    protected override void Awake() {
        base.Awake();
        _Map = GetComponent<Tilemap>();
    }

    public void Mark_O(Vector3Int pos) 
        => _Map.SetTile(pos, _Mark_O);
    
    public void Mark_X(Vector3Int pos) 
        => _Map.SetTile(pos, _Mark_X);

    public void Unmark(Vector3Int pos) 
        => _Map.SetTile(pos, null);

    public void HasMark(Vector3Int pos) 
        => _Map.HasTile(pos);
    
    public bool IsThisMoveMakeWin(Vector3Int pos) {
        bool win = false;

        Vector3Int[] dels = new Vector3Int[] {
            new(0, 1),
            new(1, 0),
            new(1, 1),
            new(1, -1) };
        TileBase pattern = _Map.GetTile(pos);

        foreach (var del in dels) {
            List<TileBase> tileList = new();
            Vector3Int itePos = pos - 4 * del;
            for (int i = 4 * 2 + 1; i > 0; --i) {
                tileList.Add(_Map.GetTile(itePos));
                itePos += del;
            }
            if (LonggestConsecutiveMatch(pattern, tileList) >= 5) {
                win = true;
            }
        }

        return win;
    }

    private int LonggestConsecutiveMatch(TileBase pattern, List<TileBase> tileList) {
        int longgest = 0;
        int curLength = 0;

        foreach (TileBase tile in tileList) {
            if (tile == pattern) curLength++;
            else curLength = 0;

            longgest = Mathf.Max(longgest, curLength);
        }

        return longgest;
    }
}