using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Update before you use it
/// </summary>
public class TouchHelper {
    public const int MAX_TOUCHES = 10;
    public const float TOUCH_DISTANCE = 10;
    #region PHASE SHORTCUT
    public const UnityEngine.InputSystem.TouchPhase PHASE_BEGAN = UnityEngine.InputSystem.TouchPhase.Began;
    public const UnityEngine.InputSystem.TouchPhase PHASE_ENDED = UnityEngine.InputSystem.TouchPhase.Ended;
    public const UnityEngine.InputSystem.TouchPhase PHASE_CANCELED = UnityEngine.InputSystem.TouchPhase.Canceled;
    public const UnityEngine.InputSystem.TouchPhase PHASE_STATIONARY = UnityEngine.InputSystem.TouchPhase.Stationary;
    public const UnityEngine.InputSystem.TouchPhase PHASE_MOVED = UnityEngine.InputSystem.TouchPhase.Moved;
    public const UnityEngine.InputSystem.TouchPhase PHASE_NONE = UnityEngine.InputSystem.TouchPhase.None;
    #endregion

    private readonly TouchData[] Touches = new TouchData[MAX_TOUCHES];
    private UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.Controls.TouchControl> RawTouch
        => Touchscreen.current.touches;

    public void UpdateToNewestState() {
        if (Touchscreen.current == null) return;
        for (int i = 0; i < Touches.Length; i++)
            Touches[i].Update(RawTouch[i]);
    }

    public TouchData this[int index]
        => Touches[index];

    public struct TouchData {
        public UnityEngine.InputSystem.TouchPhase Phase { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Delta { get; private set; }
        private UnityEngine.InputSystem.TouchPhase PrevRawPhase { get; set; }

        public void Update(UnityEngine.InputSystem.Controls.TouchControl touch) {
            if (touch.phase.value != PrevRawPhase) Phase = touch.phase.value;
            else Phase = touch.phase.value switch {
                PHASE_BEGAN => PHASE_STATIONARY,
                PHASE_ENDED => PHASE_NONE,
                PHASE_CANCELED => PHASE_NONE,
                _ => touch.phase.value,
            };
            PrevRawPhase = touch.phase.value;

            Position = touch.position.value;

            Delta = touch.delta.value;
        }
    }
}
