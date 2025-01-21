using UnityEngine;

public class VibrateHelper : Singleton<VibrateHelper> {
    public static void Vibrate() {
        if (!DataHelper.UserData.setting.vibration) return;
        Handheld.Vibrate();
    }
}