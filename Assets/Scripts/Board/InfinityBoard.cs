using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class InfinityBoard : MonoBehaviour {
    [SerializeField] private TileBase _Cell;

    private Tilemap _Map;
    private readonly List<Vector3Int> _VisibleCell = new();

    private void Awake() {
        _Map = GetComponent<Tilemap>();
    }

    private void LateUpdate() {
        RectBound bound = GetVisibleRect(Camera.main);

        _VisibleCell.RemoveAll(cell => {
            if (bound.IsInside(cell)) return false;
            _Map.SetTile(cell, null);
            return true;
        });

        Vector3Int pos = default;
        for (pos.x = bound.left; pos.x <= bound.right; ++pos.x) {
            for (pos.y = bound.bottom; pos.y <= bound.top; ++pos.y) {
                if (_VisibleCell.Contains(pos)) continue;
                _Map.SetTile(pos, _Cell);
                _VisibleCell.Add(pos);
            }
        }
    }

    private RectBound GetVisibleRect(Camera cam) => new(
        _Map.WorldToCell(cam.ScreenToWorldPoint(Vector3.zero)),
        _Map.WorldToCell(cam.ScreenToWorldPoint(new(Screen.width - 1, Screen.height - 1)))
    );
    
    private struct RectBound {
        public int left;
        public int right;
        public int bottom;
        public int top;
        
        public RectBound(Vector3Int LB, Vector3Int RT) {
            left = LB.x;
            bottom = LB.y;
            right = RT.x;
            top = RT.y;
        }

        public bool IsInside(Vector3Int point) 
            => point.x >= left 
            && point.x <= right
            && point.y >= bottom
            && point.y <= top;
    }
}
