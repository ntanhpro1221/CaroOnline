using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimplePopup : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _Text;
    [SerializeField] private Image _Background;
    private float _Duration;

    public void Init(string content, float duration) {
        _Text.text = content;
        _Duration = duration;
    }

    private void Open(TweenCallback callback) {
        float duration = 1;
        transform.DOShakeRotation(duration, 20)
            .OnComplete(callback);
    }

    private void Close() {
        float duration = 0.4f;
        _Text.DOColor(new(1, 1, 1, 0), duration);
        _Background.DOColor(new(1, 1, 1, 0), duration)
            .OnComplete(() => Destroy(gameObject));
    }

    private IEnumerator WaitDuration() {
        yield return new WaitForSecondsRealtime(_Duration);
        Close();
    }

    private void Start() {
        Open(() => StartCoroutine(WaitDuration())); 
    }
}
