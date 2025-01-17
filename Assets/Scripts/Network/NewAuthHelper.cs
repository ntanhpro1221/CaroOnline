using Firebase.Auth;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using System;
using System.Reflection;
using UnityEngine;

public class NewAuthHelper : Singleton<NewAuthHelper>, IAuthHelper {
    #region REQUIRED SETTING
    [Header("REQUIRED SETTING")]
    [SerializeField] private ClientDataObject googleClientAndroid;
    [SerializeField] private ClientDataObject googleClientStandalone;
    [Range(1024, 49151)] [SerializeField] private uint listeningPort = 17263;
    #endregion

    #region SHORT CUT
    private OpenIDConnectService OIDCService {
        get {
            if (!ServiceManager.ServiceExists<OpenIDConnectService>()) {
                OpenIDConnectService oidc = new() {
                    OidcProvider = new GoogleOidcProvider()
                };

#if UNITY_EDITOR || UNITY_STANDALONE
                oidc.OidcProvider.ClientData = googleClientStandalone.clientData;
                oidc.ServerListener.ListeningUri = $"http://localhost:{listeningPort}/";
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WSA || UNITY_VISIONOS
                oidc.OidcProvider.ClientData = GoogleClient_Android.clientData;
                oidc.RedirectURI = $"{Application.identifier.ToLower()}:/";
#else
                throw new PlatformNotSupportedException("This platform is not supported.");
#endif

                ServiceManager.RegisterService(oidc);
                ServiceManager.GetService<OpenIDConnectService>().LoginCompleted += OnGetCredentialCompleted;
            }
            
            return ServiceManager.GetService<OpenIDConnectService>();
        }
    }

    private FirebaseAuth FirebaseAuth 
        => FirebaseAuth.DefaultInstance;
    #endregion

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private IntPtr windowPos;
#endif
    private Action<bool> signedInCallback;

    protected override void Awake() {
        base.Awake();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        windowPos = Win32.GetForegroundWindow();
#endif
    }

    private async void OnGetCredentialCompleted(object sender, EventArgs e) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Win32.FocusOn(windowPos);
#endif
        await FirebaseAuth.SignInAndRetrieveDataWithCredentialAsync(GoogleAuthProvider.GetCredential(null, OIDCService.AccessToken));
        if (FirebaseAuth.CurrentUser == null) {
            Debug.LogError("Sign in not successful");
            signedInCallback?.Invoke(false);
        }

        Debug.Log("Signed in ok");
        signedInCallback?.Invoke(true);
    }
    
    public FirebaseUser User
        => FirebaseAuth.CurrentUser;

    public async void SignInWithGoogle(Action<bool> callback) {
        signedInCallback = callback;
        await OIDCService.OpenLoginPageAsync();
    }

    public void SignOut() {
        OIDCService.Logout();
        OIDCService.Cleanup();
        FirebaseAuth.SignOut();
    }
}
