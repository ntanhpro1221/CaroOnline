using UnityEngine;

public class VibrateHelper : Singleton<VibrateHelper> {
    public static void Vibrate() {
        if (AuthHelper.IsSignedIn
            ? !DataHelper.UserData.setting.vibration
            : false) return;
        Handheld.Vibrate();
    }
}