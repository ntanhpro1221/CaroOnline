using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBooster : MonoBehaviour {
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

    private const float VAL_INIT_UGS = 0.5f;
    private const float VAL_SIGNIN = 0.5f;

    private void Start() {
        StartCoroutine(Boost());
    }

    private IEnumerator Boost() {
        CurProgress = 0;
        
        // INIT UNITY SERVICE FIRST
        Task initServiceTask = UnityServices.InitializeAsync();
        while (!initServiceTask.IsCompleted) yield return null;
        CurProgress += VAL_INIT_UGS;
        LobbyHelper.Instance.Init();

        // TRY SIGN IN WITH CACHED UNITY ACCOUNT
        Task<bool> trySignInCachedUnityAccountTask = AuthHelper.TryCachedSignInWithUnityAsync();
        while (!trySignInCachedUnityAccountTask.IsCompleted) yield return null;
        CurProgress += VAL_SIGNIN;
        
        LoadSceneHelper.LoadScene(
            trySignInCachedUnityAccountTask.Result ?
            "LobbyScene" :
            "SignInScene");
    }
}
