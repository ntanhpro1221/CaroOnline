using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BasePopup))]
public class ListFollowedPlayerWindowUI : MonoBehaviour {
    [SerializeField] private GameObject _ElementObj;
    [SerializeField] private RectTransform _ElementHolder;

    public async void Init() {
        foreach (string id_firebase in DataHelper.UserData.followed_player_id_firebase)
            Add(await DataHelper.LoadUserDataAsync(id_firebase));
    }

    private void Add(UserData userData) {
        ElementFollowedPlayerUI elementUI = Instantiate(_ElementObj, _ElementHolder).GetComponent<ElementFollowedPlayerUI>();
        elementUI.WithProfile(userData);
    }
}
