using Firebase.Auth;
using System;

public interface IAuthHelper {
    FirebaseUser User { get; }
    void SignInWithGoogle(Action<bool> callback);
    void SignOut();
}
