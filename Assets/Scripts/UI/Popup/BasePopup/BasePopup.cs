using DG.Tweening;
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
    [Space]
    [Header("TITLE")]
    [SerializeField] private TextMeshProUGUI _TitleTxt;
    [Space]
    [Header("CONTENT")]
    [SerializeField] private LayoutElement _ContentElement;
    [SerializeField] private TextMeshProUGUI _ContentTxt;
    [Space]
    [Header("BUTTON")]
    [SerializeField] private LayoutElement _ButtonElement;
    [SerializeField] private RectTransform _ButtonRoot;

    private void Start() {
        Appear();
        _BackgroundBtn.onClick.AddListener(Disappear);
        _CloseBtn.onClick.AddListener(Disappear);
    }

    protected virtual void Appear() {
        float duration = 0.2f;

        _BackgroundImg.DOColor(new(0, 0, 0, 0.5f), duration);

        _WindowRoot
            .DOMove(Vector3.zero, duration)
            .SetEase(Ease.OutBack);
    }

    protected virtual void Disappear() {
        float duration = 0.2f;

        _BackgroundBtn.targetGraphic.DOColor(new(0, 0, 0, 0), duration);

        _WindowRoot
            .DOMove(new Vector3(0, 2 * Camera.main.orthographicSize, 0), duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
    
    public BasePopup WithTitle(string title) {
        _TitleTxt.text = title;
        return this;
    }
    
    public BasePopup WithContent(string content) {
        if (content == null) return this;
        _ContentElement.gameObject.SetActive(true);
        _ContentTxt.gameObject.SetActive(true);
        _ContentTxt.text = content;
        return this;
    }

    protected BasePopup WithCustomContent() {
        _ContentElement.gameObject.SetActive(true);
        _ContentTxt.gameObject.SetActive(false);
        return this;
    }

    public BasePopup WithButton(SimpleButton.CreateOption option, bool close = false) {
        _ButtonElement.gameObject.SetActive(true);
        if (close) option.callback += Disappear;
        Instantiate(_SimpleBtn, _ButtonRoot).GetComponent<SimpleButton>().Build(option);
        return this;
    }
}
