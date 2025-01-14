using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoomElementUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _RoomNameTxt;
    [SerializeField] private Button _JoinBtn;

    private UnityEvent _OnJoinClicked { get; } = new();

    private void Awake() {
        _JoinBtn.onClick.AddListener(_OnJoinClicked.Invoke);
    }
    
    public RoomElementUI BuildFull(string roomName, UnityAction callback) =>
        WithRoomName(roomName).
        WithJoinCallback(callback);
 
    public RoomElementUI UpdateFull(string roomName) =>
        WithRoomName(roomName);

    public RoomElementUI WithRoomName(string roomName) {
        _RoomNameTxt.text = roomName;
        return this;
    }

    public RoomElementUI WithJoinCallback(UnityAction callback) {
        _OnJoinClicked.AddListener(callback);
        return this;
    }
}
