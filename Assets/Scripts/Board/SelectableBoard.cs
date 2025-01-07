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

        if (LeftMouse != null) {
            _CurCell = ScreenToCell(MousePos);

            if (LeftMouse.wasPressedThisFrame)
                _PressStartPoint = MousePos;

            _Map.SetColor(_CurCell.Value, LeftMouse.isPressed
                ? _PressedColor
                : _HighlightedColor);

            if (LeftMouse.wasReleasedThisFrame) {
                _Map.SetColor(_CurCell.Value, _HighlightedColor);

                if (Vector3.Distance(_PressStartPoint, MousePos) <= float.Epsilon)
                    OnCellSelected.Invoke(_CurCell.Value);
            }
        }
        
        if (Touch != null) {
            _CurCell = ScreenToCell(TouchPos);

            if (Touch.phase.value == UnityEngine.InputSystem.TouchPhase.Began)
                _PressStartPoint = _CurCell.Value;

            _Map.SetColor(_CurCell.Value, _PressedColor);

            if (Touch.phase.value == UnityEngine.InputSystem.TouchPhase.Ended) {
                _Map.SetColor(_CurCell.Value, _NormalColor);
                if (Vector3.Distance(_PressStartPoint, MousePos) <= float.Epsilon)
                    OnCellSelected.Invoke(_CurCell.Value);
            }
        }
    }
    
    private Vector2 MousePos
        => Mouse.current.position.value;

    private UnityEngine.InputSystem.Controls.ButtonControl LeftMouse
        => Mouse.current?.leftButton;
    
    private Vector2 TouchPos
        => Touch.position.value;

    private UnityEngine.InputSystem.Controls.TouchControl Touch
        => Touchscreen.current?.touches[0];

    private Vector3Int ScreenToCell(Vector3 position)
        => _Map.WorldToCell(_Cam.ScreenToWorldPoint(position));
}