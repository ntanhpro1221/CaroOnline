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

    protected override void Awake() {
        base.Awake();
        _Map = GetComponent<Tilemap>();
        _Cam = Camera.main;
    }
    
    private Vector3 _PressStartPoint;
    private bool _IsConfirmSelect = false;
    private Vector3Int? _HoveredCell;
    private Vector3Int? _PressedCell;
    private Vector3Int? _SelectedCell;

    private void ResetColorOfAllCell() {
        void ResetColorCell(Vector3Int? cell) { 
            if (cell != null) 
                _Map.SetColor(cell.Value, _NormalColor); 
        }
        ResetColorCell(_HoveredCell);
        ResetColorCell(_PressedCell);
        ResetColorCell(_SelectedCell);
    }

    private void ColorHoveredCell() {
        _HoveredCell = null;

        if (LeftMouse != null) 
            _HoveredCell = ScreenToCell(MousePos);

        if (_HoveredCell != null) 
            _Map.SetColor(_HoveredCell.Value, _HighlightedColor);
    }
    
    private void ColorSelectedCell() {
        if (_SelectedCell != null)
            _Map.SetColor(_SelectedCell.Value, _HighlightedColor);
    }

    private void ColorPressedCell() {
        _PressedCell = null;

        if (LeftMouse != null &&
            LeftMouse.isPressed)
            _PressedCell = ScreenToCell(MousePos);
        else if (Touchscreen.current != null &&
            _Touches[0].Phase is TouchHelper.PHASE_MOVED or TouchHelper.PHASE_STATIONARY)
            _PressedCell = ScreenToCell(_Touches[0].Position);

        if (_PressedCell != null) 
            _Map.SetColor(_PressedCell.Value, _PressedColor);
    }

    private void HandleTouchAction() {
        if (LeftMouse != null) {
            if (LeftMouse.wasPressedThisFrame) 
                HavePressedThisFrame(MousePos);
            if (LeftMouse.wasReleasedThisFrame)
                HaveReleasedThisFrame(MousePos);
        } else if (Touchscreen.current != null) {
            if (_Touches[0].Phase == TouchHelper.PHASE_BEGAN)
                HavePressedThisFrame(_Touches[0].Position);
            if (_Touches[0].Phase == TouchHelper.PHASE_ENDED)
                HaveReleasedThisFrame(_Touches[0].Position);
        }

        void HavePressedThisFrame(Vector3 screenPos) {
            _PressStartPoint = screenPos;
            _IsConfirmSelect = 
                _SelectedCell != null &&
                _SelectedCell.Value == ScreenToCell(screenPos);
            _SelectedCell = null;
        }
        
        void HaveReleasedThisFrame(Vector3 screenPos) {
            if (Vector2.Distance(screenPos, _PressStartPoint) <= TouchHelper.TOUCH_DISTANCE) {
                Vector3Int releasedCell = ScreenToCell(screenPos);
                if (_IsConfirmSelect) OnCellSelected.Invoke(releasedCell);
                else _SelectedCell = releasedCell;
            }
            _IsConfirmSelect = false;
        }
    }

    private void LateUpdate() {
        if (!Application.isFocused) return;

        // update touch phase (for android)
        _Touches.UpdateToNewestState();

        // clear color of current cell
        ResetColorOfAllCell();

        // update cell
        HandleTouchAction();

        // color new cell
        ColorHoveredCell();
        ColorSelectedCell();
        ColorPressedCell();
    }
    
    private Vector2 MousePos
        => Mouse.current.position.value;

    private UnityEngine.InputSystem.Controls.ButtonControl LeftMouse
        => Mouse.current?.leftButton;
    
    private Vector3Int ScreenToCell(Vector3 position)
        => _Map.WorldToCell(_Cam.ScreenToWorldPoint(position));
}