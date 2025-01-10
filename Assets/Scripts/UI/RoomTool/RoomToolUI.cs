using Unity.Services.Authentication;
using UnityEngine;

public class RoomToolUI : SceneSingleton<RoomToolUI> {
    [SerializeField] private GameObject _StatusLine_Creating;
    [SerializeField] private GameObject _StatusLine_Waiting;
    [SerializeField] private ClickyButton _Button_Create;
    [SerializeField] private ClickyButton _Button_Discard;

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
        string[] nameList = new string[] {
            "Alice",
            "Join",
            "Harry pótt?",
        };
        await LobbyHelper.Instance.CreateLobby(
            nameList[Random.Range(0, nameList.Length - 1)]
            , 1); 
    }

    private async void OnClickDiscard() {
        await LobbyHelper.Instance.DeleteHostedLobby();
    }
    
    private void OnRoomToolStatusChanged(Status status) {
        CurStatus = status;
    }

    private void Start() {
        CurStatus = Status.None;
        _Button_Create.OnAfterClick.AddListener(OnClickCreate);
        _Button_Discard.OnAfterClick.AddListener(OnClickDiscard);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.AddListener(OnRoomToolStatusChanged);
    }

    private void OnDisable() {
        _Button_Create.OnAfterClick.RemoveListener(OnClickCreate);
        _Button_Discard.OnAfterClick.RemoveListener(OnClickDiscard);
        LobbyHelper.Instance.RoomToolStatus.OnChanged.RemoveListener(OnRoomToolStatusChanged);
    }

    public enum Status {
        None,
        Creating,
        Waiting,
    }
}
