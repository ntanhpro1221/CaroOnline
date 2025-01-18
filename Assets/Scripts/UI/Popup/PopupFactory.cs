using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupFactory : Singleton<PopupFactory> {
    [SerializeField] private GameObject _BasePopupObj;
    [SerializeField] private GameObject _PopupWithInputFieldObj;
    [SerializeField] private GameObject _SimplePopupObj;
    
    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += DestroyAllPopup;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= DestroyAllPopup;
    }

    private void DestroyAllPopup(Scene arg0, LoadSceneMode arg1) {
        foreach (RectTransform child in transform) Destroy(child.gameObject);
    }

    public BasePopup ShowPopup_ManualBuild()
        => Instantiate(_BasePopupObj, transform).GetComponent<BasePopup>();

    public void ShowPopup_YesNo(
        string title,
        string content,
        ButtonField.CreateOption noBtn,
        ButtonField.CreateOption yesBtn)
        => Instantiate(_BasePopupObj, transform).GetComponent<BasePopup>()
        .WithTitle(title)
        .WithContent(content)
        .WithButton(noBtn, true)
        .WithButton(yesBtn, true);
    
    public (BasePopup, PopupContent_InputFields) ShowPopup_WithInputField() {
        GameObject obj = Instantiate(_PopupWithInputFieldObj, transform);
        return (obj.GetComponent<BasePopup>(), obj.GetComponent<PopupContent_InputFields>());
    }

    public void ShowSimplePopup(string content, float duration = 2)
        => Instantiate(_SimplePopupObj, transform).GetComponent<SimplePopup>().Init(
            content,
            duration);
}
