using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using WebSocketSharp;

public class AuthHelper : Singleton<AuthHelper> {
    private IAuthenticationService _AuthService;
    private IPlayerAccountService _UnityService;

    public void Init() {
        _UnityService = PlayerAccountService.Instance;
        _AuthService = AuthenticationService.Instance;
    }
    
    // JUST DEBUG
    public async Task<bool> SignInAnonymouslyAsync() {
        try {
            await _AuthService.SignInAnonymouslyAsync();
        } catch (Exception ex) { 
            await _AuthService.SignInAnonymouslyAsync();
        }

        print("Signed in with id: " + _AuthService.PlayerId);

        return true;
    }

    public async Task<bool> TryCachedSignInWithUnityAsync() {
        try {
            if (_AuthService.SessionTokenExists) {
                await _AuthService.SignInAnonymouslyAsync();
                return _AuthService.IsSignedIn;
            }
        } catch (Exception ex) {
            Debug.LogException(ex);
        }
        return false;
    }

    public async Task SignInWithUnityAsync() {
        if (await TryCachedSignInWithUnityAsync()) return;

        try {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            IntPtr _RedirectWindow = Win32.GetForegroundWindow();
#endif
            await _UnityService.StartSignInAsync();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Win32.FocusOn(_RedirectWindow);
#endif

            while (_UnityService.AccessToken.IsNullOrEmpty()) await Task.Delay(100);
            await _AuthService.SignInWithUnityAsync(_UnityService.AccessToken);
            
            if (_AuthService.PlayerName.IsNullOrEmpty())
                await _AuthService.UpdatePlayerNameAsync("No_Name");
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    public void SignOut() {
        _UnityService.SignOut();
        _AuthService.SignOut();
        _AuthService.ClearSessionToken();
    }
}