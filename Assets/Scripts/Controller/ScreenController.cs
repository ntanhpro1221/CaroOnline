using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenController : MonoBehaviour {
    [SerializeField] private float _ZoomSpeed = 0.5f;
    [SerializeField] private float _MinOrthoSize = 1;
    [SerializeField] private float _MaxOrthoSize = 20;
    private Camera _Cam;
    private Vector2 oldMouse; // window only

    private void Awake() {
        _Cam = Camera.main;
    }

    private void Update() {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (Mouse.current.leftButton.isPressed)
            _Cam.transform.position += 
                _Cam.ScreenToWorldPoint(oldMouse) - 
                _Cam.ScreenToWorldPoint(Mouse.current.position.value);

        _Cam.orthographicSize = Mathf.Clamp(
            _Cam.orthographicSize - Mouse.current.scroll.value.y * _ZoomSpeed, 
            _MinOrthoSize, 
            _MaxOrthoSize);

        oldMouse = Mouse.current.position.value;
#else
        print("android??");
        //var touches = Touchscreen.current.touches;
#endif
    }
}