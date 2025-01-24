using UnityEngine;
using UnityEngine.InputSystem;

public class Alice : MonoBehaviour {
    private void Update() {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame) {
            Debug.LogWarning("ESCAPE---------------");
        }        
    }
}
