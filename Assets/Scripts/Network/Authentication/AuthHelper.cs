using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthHelper : Singleton<AuthHelper> {
    private static IAuthenticationService _UnityService 
        => AuthenticationService.Instance;
    private static FirebaseAuth _FirebaseService 
        => FirebaseAuth.DefaultInstance;

    public static bool IsSignedIn => 
        UnityServices.State == ServicesInitializationState.Initialized &&
        _UnityService?.IsSignedIn == true &&
        _FirebaseService.CurrentUser != null &&
        _FirebaseService.CurrentUser.IsValid();

    public static FirebaseUser User => _FirebaseService.CurrentUser;
    public static string id_unity => _UnityService.PlayerId;

    // JUST DEBUG
    public static async Task<bool> SignInAnonymouslyAsync() {
        SignOut();
        try {
            await Task.WhenAll(
                _FirebaseService.SignInAnonymouslyAsync(),
                _UnityService.SignInAnonymouslyAsync());
        } catch (Exception ex) {
            Debug.LogException(ex);
            return false;
        }

        Debug.Log("Signed in with id: " + _UnityService.PlayerId);

        return IsSignedIn;
    }
    
    public static async Task<bool> TryCachedSignInWithUnityAsync() {
        try {
            if (!_UnityService.SessionTokenExists) return false;
            await _UnityService.SignInAnonymouslyAsync();

            if (_FirebaseService.CurrentUser == null) return false;
        } catch (Exception ex) {
            Debug.LogException(ex);
            SignOut();
            return false;
        }

        if (!_UnityService.IsSignedIn) return false;

        await DataHelper.LoadWhenSignedIn();

        return true;
    }

    public static async Task SignInWithGoogle(Action<bool> callback) {
        if (await TryCachedSignInWithUnityAsync()) return;

        SignOut();

        try {
            GoogleAuthHelper.GetIdToken(async authResponse => {
                await Task.WhenAll(
                    _UnityService.SignInWithGoogleAsync(authResponse.id_token),
                    _FirebaseService.SignInWithCredentialAsync(GoogleAuthProvider.GetCredential(authResponse.id_token, authResponse.access_token)));

                if (IsSignedIn) await DataHelper.LoadWhenSignedIn();

                callback?.Invoke(IsSignedIn);
            });
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    public static void SignOut(Action callback = null) {
        _UnityService.SignOut();
        _UnityService.ClearSessionToken();
        _FirebaseService.SignOut();
        DataHelper.ClearCachedUserData();
        callback?.Invoke();
    }
}