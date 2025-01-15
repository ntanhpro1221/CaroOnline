using UnityEngine;

public class VibrateHelper : Singleton<VibrateHelper> {
    public static void Vibrate() {
        Handheld.Vibrate();
    }
}