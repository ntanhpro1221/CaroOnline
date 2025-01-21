using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class WinLineColor : SceneSingleton<WinLineColor> {
    [SerializeField] private TileBase _Cell;
    
    private Tilemap _Map => GetComponent<Tilemap>();

    public void ColorWinLine(Vector3Int center, MarkType type) {
        _Map.color = type == MarkType.X ? Color.red : Color.blue;
        foreach (var pos in MarkHelper.Instance.LonggestConsecutiveMatch_FromThisMark(center))
            _Map.SetTile(pos, _Cell);
    }

    public void ClearColor() {
        _Map.ClearAllTiles();
    }
}
