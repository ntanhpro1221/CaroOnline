using System;
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

    public async Task<bool> TryCachedSignInWithUnityAsync() {
        if (_AuthService.SessionTokenExists) {
            await _AuthService.SignInAnonymouslyAsync();
            return true;
        }
        return false;
    }

    public async Task SignInWithUnityAsync() {
        if (await TryCachedSignInWithUnityAsync()) return;

        try {
            await _UnityService.StartSignInAsync();

            while (_UnityService.AccessToken.IsNullOrEmpty()) await Task.Delay(100);
            await _AuthService.SignInWithUnityAsync(_UnityService.AccessToken);
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }
}