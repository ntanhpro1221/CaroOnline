using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class RoomToolUI : SceneSingleton<RoomToolUI> {
    [SerializeField] private GameObject _StatusLine_Creating;
    [SerializeField] private GameObject _StatusLine_Waiting;
    [SerializeField] private ClickyButton _Button_Create;
    [SerializeField] private ClickyButton _Button_Discard;
    [SerializeField] private ClickyButton _Button_SignOut;

    private Status _CurStatus;
    public Status CurStatus {
        get => _CurStatus;
        set {
            _CurStatus = value;
            _StatusLine_Creating.SetActive(value is Status.Creating);
            _StatusLine_Waiting.SetActive(value is Status.Waiting);
            _Button_Create.gameObject.SetActive(value is Status.None or Status.Creating);
            _Button_Discard.gameObject.SetActive(value is Status.Waiting);
        }
    }
    
    private async void OnClickCreate() {
        await LobbyHelper.Instance.CreateLobby(
            AuthenticationService.Instance.PlayerName.Replace('_', ' '), 
            2); 
    }

    private async void OnClickDiscard() {
        await LobbyHelper.Instance.DeleteHostedLobby();
    }
    
    private async void OnClickSignOut() {
        AuthHelper.Instance.SignOut();
        await SceneManager.LoadSceneAsync("SignInScene"); 
    }

    private void OnRoomToolStatusChanged(Status status) {
        CurStatus = status;
    }

    private void Start() {
        CurStatus = Status.None;
        _Button_Create.OnAfterClick.AddListener(OnClickCreate);
        _Button_Discard.OnAfterClick.AddListener(OnClickDiscard);
        _Button_SignOut.OnAfterClick.AddListener(OnClickSignOut);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.AddListener(OnRoomToolStatusChanged);
    }

    private void OnDisable() {
        _Button_Create.OnAfterClick.RemoveListener(OnClickCreate);
        _Button_Discard.OnAfterClick.RemoveListener(OnClickDiscard);
        _Button_SignOut.OnAfterClick.RemoveListener(OnClickSignOut);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.RemoveListener(OnRoomToolStatusChanged);
    }

    public enum Status {
        None,
        Creating,
        Waiting,
    }
}
