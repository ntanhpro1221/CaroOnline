using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeActionHandler : Singleton<EscapeActionHandler> {
    private List<ISceneEscapeHandlable> escapeHandler;

    protected override void Awake() {
        base.Awake();
        CacheEscapeHandler();
        SceneManager.sceneLoaded += (_, _) => CacheEscapeHandler();
    }

    private void CacheEscapeHandler() {
        escapeHandler = FindObjectsByType<MonoBehaviour>(UnityEngine.FindObjectsSortMode.None).OfType<ISceneEscapeHandlable>().ToList();
    }
    
    private void DefaultOnEscape() {
        PopupFactory.ShowPopup_YesNo(
            "Bạn có muốn thoát không?",
            null,
            new() {
                content = "Thoát",
                callback = () => Application.Quit(),
                backgroundColor = Color.red,
                foregroundColor = Color.yellow,
            },
            new() {
                content = "Thôi",
                backgroundColor = Color.green,
            });
    }

    private void Update() {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame) {
            if (PopupFactory.TryRemoveTopPopup()) return;

            if (escapeHandler != null && escapeHandler.Count != 0) {
                foreach (var item in escapeHandler)
                    item.OnEscape();
                return;
            }

            DefaultOnEscape();
        } 
    }
}