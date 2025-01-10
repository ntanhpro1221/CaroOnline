using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoostToMainScene : MonoBehaviour {
    [SerializeField] private Image _ProgressFill;
    [SerializeField] private TextMeshProUGUI _ProgressText;
    private float _CurProgress = 0;
    public float CurProgress {
        get => _CurProgress;
        set {
            _CurProgress = value;
            _ProgressFill.fillAmount = value;
            _ProgressText.text = $"{(int)(value * 100)}%";
        }
    }

    private const float VAL_UGS = 0.4f;
    private const float VAL_SCENE = 0.6f;

    private void Start() {
        CurProgress = 0;
        Task loadUGSTask = LobbyHelper.Instance.WaitForInit();
        AsyncOperation loadSceneTask = SceneManager.LoadSceneAsync("LobbyScene");
        loadSceneTask.allowSceneActivation = false;

        StartCoroutine(Boost(loadUGSTask, loadSceneTask));
    }

    private IEnumerator Boost(Task loadUGSTask, AsyncOperation loadSceneTask) {
        while (!loadUGSTask.IsCompleted || !loadSceneTask.isDone) {
            CurProgress =
                VAL_UGS * (loadUGSTask.IsCompleted ? 1 : 0) +
                VAL_SCENE * loadSceneTask.progress;
            loadSceneTask.allowSceneActivation = loadUGSTask.IsCompleted;
            yield return null;
        }
    }
}
