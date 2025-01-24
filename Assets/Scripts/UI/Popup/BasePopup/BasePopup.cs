using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasePopup : MonoBehaviour {
    [SerializeField] private GameObject _SimpleBtn;
    [Space]
    [SerializeField] private Image _BackgroundImg;
    [SerializeField] private Button _BackgroundBtn;
    [Space]
    [SerializeField] private Transform _WindowRoot;
    [Space]
    [SerializeField] private Button _CloseBtn;
    [SerializeField] private GameObject _CloseBtnObj;
    [Space]
    [Header("TITLE")]
    [SerializeField] private TextMeshProUGUI _TitleTxt;
    [Space]
    [Header("CONTENT")]
    [SerializeField] private LayoutElement _ContentElement;
    [SerializeField] private TextMeshProUGUI _ContentTxt;
    [Space]
    [Header("LOADING ICON")]
    [SerializeField] private GameObject _LoadingIconElement;
    [Space]
    [Header("BUTTON")]
    [SerializeField] private LayoutElement _ButtonElement;
    [SerializeField] private RectTransform _ButtonRoot;

    private Action _ExitCallback;
    public bool Exitable { get; private set; } = true;

    private void Start() {
        Appear();
        if (Exitable) {
            _BackgroundBtn.onClick.AddListener(Disappear);
            _CloseBtn.onClick.AddListener(Disappear);
        }
    }

    protected virtual void Appear() {
        float duration = 0.2f;

        _BackgroundImg.DOColor(new(0, 0, 0, 0.5f), duration);

        _WindowRoot
            .DOMove(new(Screen.width / 2, Screen.height / 2), duration)
            .SetEase(Ease.OutBack);
    }

    public virtual void Disappear() {
        float duration = 0.2f;

        _BackgroundBtn.targetGraphic.DOColor(new(0, 0, 0, 0), duration);

        _WindowRoot
            .DOMove(new(Screen.width / 2, Screen.height * 1.5f), duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                _ExitCallback?.Invoke();
                Destroy(gameObject);
                });
    }
    
    public BasePopup WithTitle(string title) {
        _TitleTxt.text = title;
        return this;
    }
    
    public BasePopup WithTitleColor(Color color) {
        _TitleTxt.color = color;
        return this;
    }

    public BasePopup WithContent(string content) {
        _ContentElement.gameObject.SetActive(content != null);
        _ContentTxt.gameObject.SetActive(content != null);
        if (content == null) return this;
        _ContentTxt.text = content;
        return this;
    }
    
    public BasePopup WithLoadingIcon(bool enableLoadingIcon) {
        _LoadingIconElement.SetActive(enableLoadingIcon);
        return this;
    }

    public BasePopup WithContentColor(Color color) {
        _ContentTxt.color = color;
        return this;
    }
    
    public BasePopup WithButton(ButtonField.CreateOption option, bool close = false) {
        _ButtonElement.gameObject.SetActive(true);
        if (close) option.callback += Disappear;
        Instantiate(_SimpleBtn, _ButtonRoot).GetComponent<ButtonField>().Build(option);
        return this;
    }

    public BasePopup WithExitable(bool exitable) {
        _BackgroundBtn.onClick.RemoveAllListeners();
        _CloseBtn.onClick.RemoveAllListeners();
        if (exitable) {
            _BackgroundBtn.onClick.AddListener(Disappear);
            _CloseBtn.onClick.AddListener(Disappear);
        }
        _CloseBtnObj.SetActive(exitable);
        Exitable = exitable;
        return this;
    }

    public BasePopup WithoutButton() {
        foreach (RectTransform child in _ButtonRoot) Destroy(child.gameObject);
        return this;
    }

    public BasePopup WithExitCallback(Action callback) {
        _ExitCallback += callback;
        return this;
    }
}
