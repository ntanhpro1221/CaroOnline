using TMPro;
using UnityEngine;

public class RoomToolUI : SceneSingleton<RoomToolUI> {
    [Space]
    [SerializeField] private GameObject _StatusLine_Creating;

    [Space]
    [SerializeField] private GameObject _StatusLine_Waiting;
    [SerializeField] private TextMeshProUGUI _RoomCodeTxt;
    [SerializeField] private ClickyButton _Button_CopyRoomCode;

    [Space]
    [SerializeField] private GameObject _StatusLine_DoingNothing;
    [SerializeField] private ClickyButton _Button_JoinWithCode;
    [SerializeField] private ClickyButton _Button_PlayWithBot;

    [Space]
    [SerializeField] private ClickyButton _Button_Create;
    [Space]
    [SerializeField] private ClickyButton _Button_Discard;
    [Space]
    [SerializeField] private ClickyButton _Button_Setting;

    private Status _CurStatus;
    public Status CurStatus {
        get => _CurStatus;
        set {
            _CurStatus = value;
            _StatusLine_Creating.SetActive(value is Status.Creating);
            _StatusLine_Waiting.SetActive(value is Status.Waiting);
            _StatusLine_DoingNothing.SetActive(value is Status.None);
            
            _RoomCodeTxt.text = "Mã phòng: " + (LobbyHelper.Instance.JoinedLobby?.Value?.LobbyCode ?? "NULL");
            _Button_Create.gameObject.SetActive(value is Status.None or Status.Creating);
            _Button_Discard.gameObject.SetActive(value is Status.Waiting);
        }
    }
    
    private void OnClickPlayWithBot() {
        DataHelper.SceneBoostData.battle.battleMode = BattleMode.Player_Bot;
        LoadSceneHelper.LoadScene("BattleScene");
    }

    private void OnClickCopyRoomCode() {
        GUIUtility.systemCopyBuffer = LobbyHelper.Instance.JoinedLobby.Value?.LobbyCode ?? "NULL";
        PopupFactory.ShowSimplePopup("Đã copy mã phòng");
    }

    private async void OnClickCreate() {
        if (!ConnectionChecker.CachedInternetCheckResult) {
            PopupFactory.ShowPopup_PlayingOfflineMode();
            return;
        }
        await LobbyHelper.Instance.CreateLobby(
            AuthHelper.User.DisplayName, 
            2); 
    }

    private async void OnClickDiscard() {
        await LobbyHelper.Instance.DeleteHostedLobby();
    }
    
    private void OnClickSetting() {
        if (!ConnectionChecker.CachedInternetCheckResult) {
            PopupFactory.ShowPopup_PlayingOfflineMode();
            return;
        }
        PopupFactory.ShowSettingWindow();
    }
    
    private void OnClickJoinWithCode() {
        if (!ConnectionChecker.CachedInternetCheckResult) {
            PopupFactory.ShowPopup_PlayingOfflineMode();
            return;
        }
        var (popup, fields) = PopupFactory.ShowPopup_WithInputField();
        LabeledInputField field = fields.WithField("Mã phòng", "Nhập mã phòng").Item2;
        popup
            .WithTitle("Tham gia bằng mã phòng từ đối thủ của bạn")
            .WithButton(
                new() {
                    content = "Thôi",
                    backgroundColor = Color.red,
                },
                true)
            .WithButton(
                new() {
                    content = "Tham gia",
                    backgroundColor = Color.green,
                    callback = async () => await LobbyHelper.Instance.JoinLobbyByCode(field.Text),
                });
    }

    private void OnRoomToolStatusChanged(Status status) {
        CurStatus = status;
    }

    private void Start() {
        CurStatus = Status.None;
        LobbyHelper.Instance.RoomToolStatus.OnChanged.AddListener(OnRoomToolStatusChanged);

        _Button_CopyRoomCode.OnAfterClick.AddListener(OnClickCopyRoomCode);
        _Button_Create.OnAfterClick.AddListener(OnClickCreate);
        _Button_Discard.OnAfterClick.AddListener(OnClickDiscard);
        _Button_Setting.OnAfterClick.AddListener(OnClickSetting);
        _Button_JoinWithCode.OnAfterClick.AddListener(OnClickJoinWithCode);
        _Button_PlayWithBot.OnAfterClick.AddListener(OnClickPlayWithBot);
    }

    private void OnDisable() {
        LobbyHelper.Instance.RoomToolStatus.OnChanged.RemoveListener(OnRoomToolStatusChanged);
    }

    public enum Status {
        None,
        Creating,
        Waiting,
    }
}
