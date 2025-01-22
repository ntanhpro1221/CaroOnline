using System;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleAuthHelper : Singleton<GoogleAuthHelper> {
    [SerializeField] private ClientDataAsset clientData;

    private const string code_server = "https://oauth2.googleapis.com/token";
    private const string auth_server = "https://accounts.google.com/o/oauth2/v2/auth";
    private static string client_id => Instance.clientData.client_id;
    private static string client_secret => Instance.clientData.client_secret;
    private const string redirect_uri = AuthInfo.app_uri + ":";
    private const string response_type = "code";
    private const string scope = "openid%20profile%20email";
    private const string grant_type = "authorization_code";

    private static Action<AuthResponseData> cached_callback;

    public static void GetIdToken(Action<AuthResponseData> callback) {
        cached_callback = callback;
        DeepLinkHelper.StartListen(HandleRawResponseUri);
        Application.OpenURL(auth_server
        + $"?{nameof(client_id)}={client_id}"
        + $"&{nameof(redirect_uri)}={redirect_uri}"
        + $"&{nameof(response_type)}={response_type}"
        + $"&{nameof(scope)}={scope}");
    }
    
    private static async void HandleRawResponseUri(string uri) {
        WWWForm form = new();
        form.AddField("code", ExtractCode(uri));
        form.AddField(nameof(client_id), client_id);
        form.AddField(nameof(client_secret), client_secret);
        form.AddField(nameof(redirect_uri), redirect_uri);
        form.AddField(nameof(grant_type), grant_type);
        UnityWebRequest request = UnityWebRequest.Post(code_server, form);
        await request.SendWebRequest();
        cached_callback?.Invoke(JsonUtility.FromJson<AuthResponseData>(request.downloadHandler.text));
    }

    private static string ExtractCode(string uri) {
        int start = uri.IndexOf("code=") + 5;
        return uri[start..uri.IndexOf("&", start)];
    }

    [Serializable]
    public class AuthResponseData {
        public string id_token;
        public string access_token;
    }
}
