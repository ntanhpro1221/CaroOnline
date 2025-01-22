using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BattleScreenController : SceneSingleton<BattleScreenController> {
    [SerializeField] [Range(1.1f, 1.2f)] private float _ZoomSpeed = 1.1f;
    [SerializeField] private float _MinOrthoSize = 2;
    [SerializeField] private float _MaxOrthoSize = 12;
    private Camera _Cam => Camera.main;
    private Vector2? oldMouse; // window only
    
    public void FocusOn(Vector2 worldPos) {
        Vector3 CamWorldPos2D = new(worldPos.x, worldPos.y, _Cam.transform.position.z);
        _Cam.transform.DOMove(CamWorldPos2D, 0.4f).SetEase(Ease.OutQuad);
    }

    private void OnApplicationFocus(bool focus) {
        if (focus == false) {
            oldMouse = null;
        }
    }
    
    private void Update() {
        HandleMoveScreen();
        HandleZoomScreen();
    }

    private void HandleMoveScreen() {
        if (!Application.isFocused) return;

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Mouse.current != null) {
                if (Mouse.current.leftButton.isPressed) {
                    Vector2 l_OldMouse = oldMouse == null ?
                        MousePos :
                        oldMouse.Value;

                    _Cam.transform.Translate(
                        _Cam.ScreenToWorldPoint(l_OldMouse) -
                        _Cam.ScreenToWorldPoint(MousePos));
                }

                oldMouse = MousePos;
            } else if (Touchscreen.current != null) {
                if (Touches.Count < 1 ||
                    !IsMovedPhase(Touches[0])) return;

                if (Touches.Count > 1 &&
                    (IsMovedPhase(Touches[1]) || IsStationaryPhase(Touches[1]))) return;

                _Cam.transform.Translate(
                        _Cam.ScreenToWorldPoint(Touches[0].position.value - Touches[0].delta.value) -
                        _Cam.ScreenToWorldPoint(Touches[0].position.value));
            }
        }
    }

    private void HandleZoomScreen() {
        if (!Application.isFocused) return;

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Mouse.current != null) {
                // Không làm gì nếu con trỏ không ở trong màn hình
                if (!IsInside(new(0, 0, Screen.width, Screen.height), MousePos)) return;

                // Lưu lại tọa độ thế giới của con trỏ hiện tại
                Vector2 storedMousePos = _Cam.ScreenToWorldPoint(MousePos);

                // Zoom theo %
                _Cam.orthographicSize = Mathf.Clamp(
                    _Cam.orthographicSize * Mathf.Pow(_ZoomSpeed, -MouseScroll),
                    _MinOrthoSize,
                    _MaxOrthoSize);

                // Khôi phục lại tọa độ thế giới của con trỏ sau khi zoom
                _Cam.transform.Translate(storedMousePos - (Vector2)_Cam.ScreenToWorldPoint(MousePos));
            } else if (Touchscreen.current != null) {
                // Không có 2 ngón đang di chuyển thì thôi
                if (Touches.Count < 2 ||
                    (!IsMovedPhase(Touches[0]) && !IsStationaryPhase(Touches[0])) ||
                    (!IsMovedPhase(Touches[1]) && !IsStationaryPhase(Touches[1]))) return;

                Vector2 finger_0 = Touches[0].position.value;
                Vector2 finger_1 = Touches[1].position.value;
                Vector2 prev_finger_0 = finger_0 - Touches[0].delta.value;
                Vector2 prev_finger_1 = finger_1 - Touches[1].delta.value;

                // Lưu lại tọa độ thế giới của trung điểm 2 ngón tay hiện tại
                Vector2 storedPivotPos = _Cam.ScreenToWorldPoint((prev_finger_0 + prev_finger_1) / 2);

                // Zoom theo tỉ lệ khoảng cách 2 ngón tay
                _Cam.orthographicSize = Mathf.Clamp(
                    _Cam.orthographicSize *
                        Vector2.Distance(prev_finger_0, prev_finger_1) /
                        Vector2.Distance(finger_0, finger_1),
                    _MinOrthoSize,
                    _MaxOrthoSize);

                // Khôi phục lại tọa độ thế giới của trung điểm 2 ngón tay sau khi zoom
                _Cam.transform.Translate(storedPivotPos -
                    (Vector2)_Cam.ScreenToWorldPoint((Touches[0].position.value + Touches[1].position.value) / 2));
            }
        }
    }

    private bool IsMovedPhase(UnityEngine.InputSystem.Controls.TouchControl touch)
        => touch.phase.value == UnityEngine.InputSystem.TouchPhase.Moved;

    private bool IsStationaryPhase(UnityEngine.InputSystem.Controls.TouchControl touch)
        => touch.phase.value == UnityEngine.InputSystem.TouchPhase.Stationary;

    private UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.Controls.TouchControl> Touches
        => Touchscreen.current.touches;

    private float MouseScroll
        => 3 * Mouse.current.scroll.x.value
        + Mouse.current.scroll.y.value;

    private Vector2 MousePos
        => Mouse.current.position.value;

    private bool IsInside(Rect rect, Vector2 pos)
        => pos.x >= rect.x
        && pos.y >= rect.y
        && pos.x < rect.x + rect.width
        && pos.y < rect.y + rect.height;
}