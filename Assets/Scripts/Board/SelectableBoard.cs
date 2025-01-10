using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class SelectableBoard : SceneSingleton<SelectableBoard> {
    public UnityEvent<Vector3Int> OnCellSelected { get; } = new();

    [SerializeField] private Color _NormalColor;
    [SerializeField] private Color _HighlightedColor;
    [SerializeField] private Color _PressedColor;
    private Tilemap _Map;
    private Camera _Cam;
    private readonly TouchHelper _Touches = new();
    private Vector3Int? _CurCell;
    private Vector3 _PressStartPoint;

    protected override void Awake() {
        base.Awake();
        _Map = GetComponent<Tilemap>();
        _Cam = Camera.main;
    }

    private void LateUpdate() {
        if (_CurCell != null) {
            _Map.SetColor(_CurCell.Value, _NormalColor);
            _CurCell = null;
        }
        
        if (!Application.isFocused) return;

        if (LeftMouse != null) {
            _CurCell = ScreenToCell(MousePos);

            if (LeftMouse.wasPressedThisFrame)
                _PressStartPoint = MousePos;

            _Map.SetColor(_CurCell.Value, LeftMouse.isPressed
                ? _PressedColor
                : _HighlightedColor);

            if (LeftMouse.wasReleasedThisFrame) {
                _Map.SetColor(_CurCell.Value, _HighlightedColor);

                if (Vector3.Distance(_PressStartPoint, MousePos) <= TouchHelper.TOUCH_DISTANCE)
                    OnCellSelected.Invoke(_CurCell.Value);
            }
        } else if (Touchscreen.current != null) {
            _Touches.UpdateToNewestState();

            _CurCell = ScreenToCell(_Touches[0].Position);

            if (_Touches[0].Phase == TouchHelper.PHASE_BEGAN)
                _PressStartPoint = _Touches[0].Position;
            
            if (_Touches[0].Phase is TouchHelper.PHASE_MOVED or TouchHelper.PHASE_STATIONARY)
                _Map.SetColor(_CurCell.Value, _PressedColor);

            if (_Touches[0].Phase == TouchHelper.PHASE_ENDED) {
                _Map.SetColor(_CurCell.Value, _NormalColor);

                if (Vector3.Distance(_PressStartPoint, _Touches[0].Position) <= TouchHelper.TOUCH_DISTANCE) {
                    OnCellSelected.Invoke(_CurCell.Value);
                }
            }
        }
    }
    
    private Vector2 MousePos
        => Mouse.current.position.value;

    private UnityEngine.InputSystem.Controls.ButtonControl LeftMouse
        => Mouse.current?.leftButton;
    
    private Vector3Int ScreenToCell(Vector3 position)
        => _Map.WorldToCell(_Cam.ScreenToWorldPoint(position));
}