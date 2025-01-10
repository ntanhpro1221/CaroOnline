using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimplePopupUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _Text;
    [SerializeField] private Image _Background;
    private float _Duration;

    public void Init(string content, float duration) {
        _Text.text = content;
        _Duration = duration;
    }

    private void Open(TweenCallback callback) {
        transform.DOShakeRotation(2, 20)
            .OnComplete(callback);
    }

    private void Close() {
        _Background.DOColor(new(1, 1, 1, 0), 0.4f)
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
