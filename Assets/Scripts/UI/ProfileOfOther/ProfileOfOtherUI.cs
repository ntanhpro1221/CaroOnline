using TMPro;
using UnityEngine;

public class ProfileOfOtherUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _NameTxt;
    [SerializeField] private TextField _RankTxt;
    [SerializeField] private TextField _EloTxt;
    [SerializeField] private ButtonField _FollowBtn;
    [SerializeField] private ButtonField _UnfollowBtn;
    
    private EFollowStatus _FollowStatus;
    private EFollowStatus FollowStatus {
        get => _FollowStatus;
        set {
            _FollowStatus = value;
            _FollowBtn.gameObject.SetActive(value == EFollowStatus.Nope);
            _UnfollowBtn.gameObject.SetActive(value == EFollowStatus.Followed);
        }
    }

    public void Init(UserData data, int? elo = null) {
        _NameTxt.text = data.name;
        _RankTxt
            .WithContent(DataHelper.GetRankOfElo(data.elo).Name)
            .WithColor(DataHelper.GetRankOfElo(data.elo).Color);
        _EloTxt.WithContent("Elo: " + (elo ?? data.elo));

        _FollowBtn.WithCallback(async () => {
            DataHelper.UserData.followed_player_id_firebase.Add(data.id_firebase);
            await DataHelper.SaveCurrentUserDataAsync();
            PopupFactory.ShowSimplePopup("Đã theo dõi " + data.name);
            FollowStatus = EFollowStatus.Followed;
        });

        _UnfollowBtn.WithCallback(async () => {
            DataHelper.UserData.followed_player_id_firebase.Remove(data.id_firebase);
            await DataHelper.SaveCurrentUserDataAsync();
            PopupFactory.ShowSimplePopup("Đã bỏ theo dõi " + data.name);
            FollowStatus = EFollowStatus.Nope;
        });

        FollowStatus = DataHelper.UserData.followed_player_id_firebase.Contains(data.id_firebase)
            ? EFollowStatus.Followed
            : EFollowStatus.Nope;
    }

    public enum EFollowStatus {
        Nope,
        Followed
    }
}
