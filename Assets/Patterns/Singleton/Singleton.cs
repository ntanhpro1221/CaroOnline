using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T m_Instance;
    public static T Instance 
        => m_Instance ??= FindFirstObjectByType<T>();
    protected virtual void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
