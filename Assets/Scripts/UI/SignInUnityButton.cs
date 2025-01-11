using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ClickyButton))]
public class SignInUnityButton : MonoBehaviour {
    private IEnumerator OnClickSignInUnity() {
        Task signInTask = AuthHelper.Instance.SignInWithUnityAsync();
        while (!signInTask.IsCompleted) yield return null;

        if (!AuthenticationService.Instance.IsSignedIn) {
            PopupFactory.Instance.ShowPopup(
                "Đăng nhập không thành công",
                signInTask.Status + ": " + signInTask.Exception?.Message,
                "Trở lại", null, true,
                "Thử lại", null, true);
        } else {
            PopupFactory.Instance.ShowSimplePopup("Thành công");
            SceneManager.LoadScene("LobbyScene");
        }
    }

    private void Awake() {
        GetComponent<ClickyButton>().OnAfterClick.AddListener(() => 
            StartCoroutine(OnClickSignInUnity()));
    }
}
