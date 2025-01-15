using UnityEngine;

public class PopupFactory : Singleton<PopupFactory> {
    private Transform _CanvasTrans;
    private Transform CanvasTrans
        => _CanvasTrans ??= GameObject.Find("Canvas").transform;

    [SerializeField] private GameObject _BasePopupObj;
    [SerializeField] private GameObject _SimplePopupObj;

    public BasePopup ShowPopup_ManualBuild()
        => Instantiate(_BasePopupObj, CanvasTrans).GetComponent<BasePopup>();

    public void ShowPopup_YesNo(
        string title,
        string content,
        SimpleButton.CreateOption noBtn,
        SimpleButton.CreateOption yesBtn)
        => Instantiate(_BasePopupObj, CanvasTrans).GetComponent<BasePopup>()
        .WithTitle(title)
        .WithContent(content)
        .WithButton(noBtn, true)
        .WithButton(yesBtn, true);

    public void ShowSimplePopup(string content, float duration = 2)
        => Instantiate(_SimplePopupObj, CanvasTrans).GetComponent<SimplePopup>().Init(
            content,
            duration);
}
