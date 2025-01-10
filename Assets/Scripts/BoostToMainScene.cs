using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoostToMainScene : MonoBehaviour {
    private async void Start() {
        await LobbyHelper.Instance.WaitForInit();
        SceneManager.LoadScene("LobbyScene");
    }
}
