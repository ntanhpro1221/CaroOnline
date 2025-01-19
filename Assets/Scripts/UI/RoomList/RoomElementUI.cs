using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoomElementUI : MonoBehaviour {
    [SerializeField] private ButtonField _ProfileBtn;
    [SerializeField] private ButtonField _PlayBtn;

    private UnityEvent _OnProfileClicked { get; } = new();
    private UnityEvent _OnPlayClicked { get; } = new();

    private void Awake() {
        _ProfileBtn.WithCallback(_OnProfileClicked.Invoke);
        _PlayBtn.WithCallback(_OnPlayClicked.Invoke);
    }
    
    public RoomElementUI WithProfile(UserData data) {
        _ProfileBtn.WithContent(data.name);
        _ProfileBtn.WithCallback(() => PopupFactory.ShowProfileOfOther(data));
        return this;
    }

    public RoomElementUI WithPlayCallback(UnityAction callback) {
        _OnPlayClicked.AddListener(callback);
        return this;
    }
}
