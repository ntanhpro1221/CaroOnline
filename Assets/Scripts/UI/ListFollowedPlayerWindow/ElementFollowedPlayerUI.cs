using UnityEngine;

public class ElementFollowedPlayerUI : MonoBehaviour {
    [SerializeField] private ButtonField _ProfileBtn;
    [SerializeField] private ButtonField _UnfollowBtn;

    public ElementFollowedPlayerUI WithProfile(UserData userData) {
        _ProfileBtn
            .WithContent(userData.name)
            .WithCallback(() => {
                PopupFactory.ShowProfileOfOther(userData).Item1.WithExitCallback(async () => {
                    if (!DataHelper.UserData.followed_player_id_firebase.Contains(userData.id_firebase)) {
                        Destroy(gameObject);
                        DataHelper.UserData.followed_player_id_firebase.Remove(userData.id_firebase);
                        await DataHelper.SaveCurrentUserDataAsync();
                    }
                });
            });

        _UnfollowBtn
            .WithCallback(() => {
                PopupFactory.ShowPopup_YesNo(
                    $"Bạn có chắc muốn hủy theo dõi {userData.name} không?",
                    null,
                    new() {
                        content = "Thôi",
                        backgroundColor = Color.red,
                        foregroundColor = Color.yellow,
                    },
                    new() {
                        content = "Chắc",
                        backgroundColor = Color.green,
                        callback = async () => {
                            Destroy(gameObject);
                            DataHelper.UserData.followed_player_id_firebase.Remove(userData.id_firebase);
                            await DataHelper.SaveCurrentUserDataAsync();
                        }
                    });
            });

        return this;
    }
}
