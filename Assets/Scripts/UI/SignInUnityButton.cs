using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

[RequireComponent(typeof(ClickyButton))]
public class SignInUnityButton : MonoBehaviour {
    [SerializeField] private GameObject _LoadingBar;

    private IEnumerator OnClickSignInUnity() {
        _LoadingBar.SetActive(true);

        bool isSignInCompleted = false;
        Task _ = AuthHelper.SignInWithGoogle(success => isSignInCompleted = true);
        while (!isSignInCompleted) yield return null;

        _LoadingBar.SetActive(false);

        if (!AuthenticationService.Instance.IsSignedIn) {
            PopupFactory.ShowPopup_ManualBuild()
                .WithTitle("Đăng nhập không thành công")
                .WithButton(
                    new() {
                        content = "Thử lại",
                        backgroundColor = Color.green,
                    },
                    true
                );
        } else {
            PopupFactory.ShowSimpleNotification("Thành công");
            LoadSceneHelper.LoadScene("LobbyScene");
        }
    }

    private void Awake() {
        GetComponent<ClickyButton>().OnAfterClick.AddListener(() => 
            StartCoroutine(OnClickSignInUnity()));
    }
}
