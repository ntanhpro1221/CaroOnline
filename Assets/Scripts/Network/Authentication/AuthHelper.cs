using Firebase.Auth;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using WebSocketSharp;

public class AuthHelper {
    private static IAuthenticationService _UnityService 
        => AuthenticationService.Instance;
    private static FirebaseAuth _FirebaseService 
        => FirebaseAuth.DefaultInstance;

    public static bool IsSignedIn 
        => _UnityService.IsSignedIn &&
        _FirebaseService.CurrentUser != null;

    public static FirebaseUser User => _FirebaseService.CurrentUser;

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
                callback?.Invoke(IsSignedIn);
            });
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    public static void SignOut() {
        _UnityService.SignOut();
        _UnityService.ClearSessionToken();
        _FirebaseService.SignOut();
    }
}