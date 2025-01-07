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

    public void Mark_O(Vector3Int pos) => _Map.SetTile(pos, _Mark_O);
    
    public void Mark_X(Vector3Int pos) => _Map.SetTile(pos, _Mark_X);

    public void Unmark(Vector3Int pos) => _Map.SetTile(pos, null);
}