using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class GameBooster : MonoBehaviour {
    [SerializeField] private Image _ProgressFill;
    [SerializeField] private TextMeshProUGUI _ProgressText;
    private float _CurProgressDoTween = 0;
    private float _CurProgressReal = 0;
    public float CurProgressReal {
        get => _CurProgressReal;
        set {
            _CurProgressReal = value;
            DOTween.To(
                () => _CurProgressDoTween,
                value => {
                    _CurProgressDoTween = value;
                    _ProgressFill.fillAmount = value;
                    _ProgressText.text = $"{(int)(100 * value)}%";
                }, value, 0.7f);
        }
    }

    private const float VAL_INIT_UGS = 0.3f;
    private const float VAL_CHECK_CONNECTION = 0.3f;
    private const float VAL_SIGNIN = 0.4f;

    private void Start() {
        StartCoroutine(Boost());
    }

    private IEnumerator Boost() {
        CurProgressReal = 0;

        // INIT UNITY SERVICE FIRST
        if (UnityServices.State != ServicesInitializationState.Initialized) {
            Task initServiceTask = UnityServices.InitializeAsync();
            while (!initServiceTask.IsCompleted) yield return null;
        }
        CurProgressReal += VAL_INIT_UGS;
        LobbyHelper.Instance.Init();

        // CHECK CONNECTION
        Task<bool> checkConnectionTask = ConnectionChecker.CheckConnection();
        while (!checkConnectionTask.IsCompleted) yield return null;
        CurProgressReal += VAL_CHECK_CONNECTION;
        if (!checkConnectionTask.Result) {
            if (AuthHelper.User != null) {
                PopupFactory.ShowPopup_YesNo(
                    "Không có kết nối!",
                    $"Bạn có muốn tiếp tục với tài khoản {AuthHelper.User.DisplayName} không?",
                    new() {
                        content = "Tiếp tục",
                        callback = () => LoadSceneHelper.LoadScene("LobbyScene"),
                        backgroundColor = Color.yellow,
                    },
                    new() {
                        content = "Thử lại",
                        callback = () => StartCoroutine(Boost()),
                        backgroundColor = Color.green,
                    }).WithExitable(false);
            } else {
                PopupFactory.ShowPopup_YesNo(
                    "Không có kết nối!",
                    $"Vui lòng kiểm tra internet và thử lại",
                    new() {
                        content = "Thôi",
                        callback = Application.Quit,
                        backgroundColor = Color.red,
                        foregroundColor = Color.yellow,
                    },
                    new() {
                        content = "Thử lại",
                        callback = () => StartCoroutine(Boost()),
                        backgroundColor = Color.green,
                    }).WithExitable(false);
            }
            yield break;
        }

        // TRY SIGN IN WITH CACHED UNITY ACCOUNT
        Task<bool> trySignInCachedUnityAccountTask = AuthHelper.TryCachedSignInWithUnityAsync();
        while (!trySignInCachedUnityAccountTask.IsCompleted) yield return null;
        CurProgressReal += VAL_SIGNIN;
        
        LoadSceneHelper.LoadScene(
            trySignInCachedUnityAccountTask.Result ?
            "LobbyScene" :
            "SignInScene");
    }
}
