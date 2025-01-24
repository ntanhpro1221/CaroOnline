using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        void DestroyAllUI() {
            foreach (RectTransform child in Instance.transform) Destroy(child.gameObject);
            _PopupList.Clear();
        }

        SceneManager.sceneLoaded += (_, _) => DestroyAllUI();
    }

    #region POPUP
    private static List<BasePopup> _PopupList = new();

    public static bool TryRemoveTopPopup() {
        if (_PopupList.Count == 0) return false;
        BasePopup popup = _PopupList.Last();
        if (!popup.Exitable) return false;
        popup.Disappear();
        return true;
    }

    private static BasePopup SpawnPopup(GameObject popupObj) {
        BasePopup popup = Instantiate(popupObj, Instance.transform).GetComponent<BasePopup>();
        _PopupList.Add(popup);
        popup.WithExitCallback(() => _PopupList.Remove(popup));
        return popup;
    }
    
    private static (BasePopup, T) SpawnExtendedPopup<T>(GameObject popupObj) where T : MonoBehaviour {
        BasePopup popup = SpawnPopup(popupObj);
        return (popup, popup.GetComponent<T>());
    }

    public static BasePopup ShowPopup_ManualBuild()
        => SpawnPopup(Instance._BasePopupObj);

    public static BasePopup ShowPopup_YesNo(string title, string content, ButtonField.CreateOption noBtn, ButtonField.CreateOption yesBtn)
        => SpawnPopup(Instance._BasePopupObj)
        .WithTitle(title)
        .WithContent(content)
        .WithButton(noBtn, true)
        .WithButton(yesBtn, true);

    public static (BasePopup, PopupContent_InputFields) ShowPopup_WithInputField()
        => SpawnExtendedPopup<PopupContent_InputFields>(Instance._PopupWithInputFieldObj);

    public static void ShowSettingWindow()
        => SpawnExtendedPopup<SettingWindowUI>(Instance._SettingWindowObj).Item2.Init();

    public static (BasePopup, ProfileOfOtherUI) ShowProfileOfOther(UserData data, int? elo = null) {
        var popup = SpawnExtendedPopup<ProfileOfOtherUI>(Instance._ProfileOfOtherObj);
        popup.Item2.Init(data, elo);
        return popup;
    }

    public static void ShowListFollowedPlayer()
        => SpawnExtendedPopup<ListFollowedPlayerWindowUI>(Instance._ListFollowedPlayerObj).Item2.Init();
    #endregion

    #region NOTIFICATION
    public static void ShowSimpleNotification(string content, float duration = 2)
        => Instantiate(Instance._SimplePopupObj, Instance.transform).GetComponent<SimplePopup>().Init(
            content,
            duration);

    public static void ShowNotification_PlayingOffline()
        => ShowSimpleNotification("Bạn đang chơi ở chế độ offline");
    #endregion
}
