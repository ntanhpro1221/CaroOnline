using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupFactory : Singleton<PopupFactory> {
    [SerializeField] private GameObject _BasePopupObj;
    [SerializeField] private GameObject _PopupWithInputFieldObj;
    [SerializeField] private GameObject _SimplePopupObj;
    [SerializeField] private GameObject _SettingWindowObj;
    
    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += DestroyAllPopup;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= DestroyAllPopup;
    }

    private static void DestroyAllPopup(Scene arg0, LoadSceneMode arg1) {
        foreach (RectTransform child in Instance.transform) Destroy(child.gameObject);
    }

    public static BasePopup ShowPopup_ManualBuild()
        => Instantiate(Instance._BasePopupObj, Instance.transform).GetComponent<BasePopup>();

    public static void ShowPopup_YesNo(
        string title,
        string content,
        ButtonField.CreateOption noBtn,
        ButtonField.CreateOption yesBtn)
        => Instantiate(Instance._BasePopupObj, Instance.transform).GetComponent<BasePopup>()
        .WithTitle(title)
        .WithContent(content)
        .WithButton(noBtn, true)
        .WithButton(yesBtn, true);
    
    public static (BasePopup, PopupContent_InputFields) ShowPopup_WithInputField() {
        GameObject obj = Instantiate(Instance._PopupWithInputFieldObj, Instance.transform);
        return (obj.GetComponent<BasePopup>(), obj.GetComponent<PopupContent_InputFields>());
    }

    public static void ShowSimplePopup(string content, float duration = 2)
        => Instantiate(Instance._SimplePopupObj, Instance.transform).GetComponent<SimplePopup>().Init(
            content,
            duration);

    public static void ShowSettingWindow()
        => Instantiate(Instance._SettingWindowObj, Instance.transform).GetComponent<SettingWindowUI>().Init();
}
