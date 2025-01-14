using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour {
    [SerializeField] private Button _BackgroundBtn;
    [SerializeField] private Button _CloseBtn;

    [SerializeField] private TextMeshProUGUI _TitleTxt;
    [SerializeField] private TextMeshProUGUI _ContentTxt;
    [SerializeField] private TextMeshProUGUI _NegativeTxt;
    [SerializeField] private TextMeshProUGUI _PossitiveTxt;
    [SerializeField] private Button _NegativeBtn;
    [SerializeField] private Button _PossitiveBtn;

    [SerializeField] private RectTransform _WindowTrans;

    public void Init(
        string title, 
        string content, 
        string negativeTxt, 
        UnityAction negativeCallback, 
        bool closeAfterNegative,
        string possitiveTxt,
        UnityAction possitiveCallback,
        bool closeAfterPossitive) {

        _TitleTxt.text = title;
        _ContentTxt.text = content;

        _NegativeTxt.text = negativeTxt;
        _NegativeBtn.onClick.AddListener(
            closeAfterNegative ?
            () => Close(negativeCallback) :
            negativeCallback);

        _PossitiveTxt.text = possitiveTxt;
        _PossitiveBtn.onClick.AddListener(
            closeAfterPossitive ?
            () => Close(possitiveCallback) :
            possitiveCallback);
    }

    private void Close(UnityAction callback = null) {
        float duration = 0.2f;

        _BackgroundBtn.targetGraphic.DOColor(new(0, 0, 0, 0), duration);

        _WindowTrans
            .DOMove(_WindowTrans.position + new Vector3(0, 2 * Camera.main.orthographicSize, 0), duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                callback?.Invoke();
                Destroy(gameObject);
            });
    }
    
    private void Open() {
        float duration = 0.2f;

        _BackgroundBtn.targetGraphic.DOColor(new(0, 0, 0, 0.5f), duration);

        _WindowTrans
            .DOMove(_WindowTrans.position - new Vector3(0, _WindowTrans.position.y, 0), duration)
            .SetEase(Ease.OutBack);
    }

    private void Awake() {
        _BackgroundBtn.onClick.AddListener(() => Close());
        _CloseBtn.onClick.AddListener(() => Close());
    }

    private void Start() {
        Open();
    }
}
