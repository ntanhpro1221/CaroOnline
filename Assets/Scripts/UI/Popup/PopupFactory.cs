using UnityEngine;
using UnityEngine.Events;

public class PopupFactory : Singleton<PopupFactory> {
    private Transform _CanvasTrans;
    private Transform CanvasTrans
        => _CanvasTrans ??= GameObject.Find("Canvas").transform;

    [SerializeField] private GameObject _PopupObj;
    [SerializeField] private GameObject _SimplePopupObj;

    public void ShowPopup(
        string title,
        string content,
        string negativeTxt,
        UnityAction negativeCallback,
        bool closeAfterNegative,
        string possitiveTxt,
        UnityAction possitiveCallback,
        bool closeAfterPossitive) 
        => Instantiate(_PopupObj, CanvasTrans).GetComponent<PopupUI>().Init(
            title,
            content,
            negativeTxt,
            negativeCallback,
            closeAfterNegative,
            possitiveTxt,
            possitiveCallback,
            closeAfterPossitive);

    public void ShowSimplePopup(string content, float duration = 2)
        => Instantiate(_SimplePopupObj, CanvasTrans).GetComponent<SimplePopupUI>().Init(
            content,
            duration);
}
