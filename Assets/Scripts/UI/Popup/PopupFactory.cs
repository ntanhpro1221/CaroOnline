﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupFactory : Singleton<PopupFactory> {
    [SerializeField] private GameObject _BasePopupObj;
    [SerializeField] private GameObject _PopupWithInputFieldObj;
    [SerializeField] private GameObject _SimplePopupObj;
    [SerializeField] private GameObject _SettingWindowObj;
    [SerializeField] private GameObject _ProfileOfOtherObj;
    [SerializeField] private GameObject _ListFollowedPlayerObj;

    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += DestroyAllPopup;
    }

    private static void DestroyAllPopup(Scene arg0, LoadSceneMode arg1) {
        foreach (RectTransform child in Instance.transform) Destroy(child.gameObject);
    }

    public static BasePopup ShowPopup_ManualBuild()
        => Instantiate(Instance._BasePopupObj, Instance.transform).GetComponent<BasePopup>();

    public static BasePopup ShowPopup_YesNo(string title, string content, ButtonField.CreateOption noBtn, ButtonField.CreateOption yesBtn)
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

    public static (BasePopup, ProfileOfOtherUI) ShowProfileOfOther(UserData data, int? elo = null) {
        GameObject obj = Instantiate(Instance._ProfileOfOtherObj, Instance.transform);
        return (
            obj.GetComponent<BasePopup>(),
            obj.GetComponent<ProfileOfOtherUI>().Init(data, elo));
    }

    public static void ShowPopup_PlayingOfflineMode()
        => ShowSimplePopup("Bạn đang chơi ở chế độ offline");

    public static void ShowListFollowedPlayer()
        => Instantiate(Instance._ListFollowedPlayerObj, Instance.transform).GetComponent<ListFollowedPlayerWindowUI>().Init();
}
