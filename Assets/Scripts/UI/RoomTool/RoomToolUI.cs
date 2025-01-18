using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomToolUI : SceneSingleton<RoomToolUI> {
    [Space]
    [SerializeField] private GameObject _StatusLine_Creating;
    [Space]
    [SerializeField] private GameObject _StatusLine_Waiting;
    [SerializeField] private TextMeshProUGUI _RoomCodeTxt;
    [SerializeField] private ClickyButton _Button_CopyRoomCode;
    [Space]
    [SerializeField] private ClickyButton _Button_Create;
    [Space]
    [SerializeField] private ClickyButton _Button_Discard;
    [Space]
    [SerializeField] private ClickyButton _Button_Setting;
    [Space]
    [SerializeField] private ClickyButton _Button_JoinWithCode;

    private Status _CurStatus;
    public Status CurStatus {
        get => _CurStatus;
        set {
            _CurStatus = value;
            _StatusLine_Creating.SetActive(value is Status.Creating);
            _StatusLine_Waiting.SetActive(value is Status.Waiting);
            _RoomCodeTxt.text = "Mã phòng: " + (LobbyHelper.Instance.JoinedLobby.Value?.LobbyCode ?? "NULL");
            _Button_Create.gameObject.SetActive(value is Status.None or Status.Creating);
            _Button_Discard.gameObject.SetActive(value is Status.Waiting);
            _Button_JoinWithCode.gameObject.SetActive(value is Status.None);
        }
    }
    
    private void OnClickCopyRoomCode() {
        GUIUtility.systemCopyBuffer = LobbyHelper.Instance.JoinedLobby.Value?.LobbyCode ?? "NULL";
        PopupFactory.ShowSimplePopup("Đã copy mã phòng");
    }

    private async void OnClickCreate() {
        await LobbyHelper.Instance.CreateLobby(
            AuthHelper.User.DisplayName, 
            2); 
    }

    private async void OnClickDiscard() {
        await LobbyHelper.Instance.DeleteHostedLobby();
    }
    
    private void OnClickSignOut() {
        PopupFactory.ShowSettingWindow();
    }
    
    private void OnClickJoinWithCode() {
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
        _Button_CopyRoomCode.OnAfterClick.AddListener(OnClickCopyRoomCode);
        _Button_Create.OnAfterClick.AddListener(OnClickCreate);
        _Button_Discard.OnAfterClick.AddListener(OnClickDiscard);
        _Button_Setting.OnAfterClick.AddListener(OnClickSignOut);
        _Button_JoinWithCode.OnAfterClick.AddListener(OnClickJoinWithCode);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.AddListener(OnRoomToolStatusChanged);
    }

    private void OnDisable() {
        _Button_CopyRoomCode.OnAfterClick.RemoveListener(OnClickCopyRoomCode);
        _Button_Create.OnAfterClick.RemoveListener(OnClickCreate);
        _Button_Discard.OnAfterClick.RemoveListener(OnClickDiscard);
        _Button_Setting.OnAfterClick.RemoveListener(OnClickSignOut);
        _Button_JoinWithCode.OnAfterClick.RemoveListener(OnClickJoinWithCode);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.RemoveListener(OnRoomToolStatusChanged);
    }

    public enum Status {
        None,
        Creating,
        Waiting,
    }
}
