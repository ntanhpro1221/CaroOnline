using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundHelper : Singleton<SoundHelper> {
    private AudioSource _AudioSource;
    private AudioSource AudioSource => _AudioSource ??= GetComponent<AudioSource>();
    [SerializeField] private SoundTable _SoundTable;

    public static void Play(SoundType soundType) 
        => Instance.AudioSource.PlayOneShot(Instance._SoundTable[soundType]);

    public static void Play(SoundType soundType, float volumeScale)
        => Instance.AudioSource.PlayOneShot(Instance._SoundTable[soundType], volumeScale);
}
