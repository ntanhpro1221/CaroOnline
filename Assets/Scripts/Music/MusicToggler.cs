using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicToggler : MonoBehaviour {
    private AudioSource MusicSource 
        => GetComponent<AudioSource>();

    private void Start() {
        DataHelper.OnUserDataChanged.AddListener(OnUserDataChanged);
        OnUserDataChanged(DataHelper.UserData);
    }

    private void OnDisable() {
        DataHelper.OnUserDataChanged.RemoveListener(OnUserDataChanged);
    }

    private void OnUserDataChanged(UserData userData) {
        if (userData.setting.music) MusicSource.UnPause();
        else MusicSource.Pause();
    }
}
