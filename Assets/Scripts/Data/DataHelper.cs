using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Timeline;

public class DataHelper : Singleton<DataHelper> {
    public static DatabaseReference RootDB 
        => FirebaseDatabase.DefaultInstance.RootReference;

    public static async Task LoadWhenSignedIn() {
        await LoadCurrentUserDataAsync();
        await LoadRankDataConfigAsync();
    }

    #region SCENE BOOST DATA
    [SerializeField] private SceneBoostData m_SceneBoostData = new();

    public static SceneBoostData SceneBoostData
        => Instance.m_SceneBoostData;
    #endregion

    #region UNITY TO FIREBASE
    public static DatabaseReference UnityToFireBaseDB 
        => RootDB.Child("Unity To Firebase");

    public static async Task<string> UnityToFirebase(string id_unity)
        => await LoadObjectAsync<string>(UnityToFireBaseDB.Child(id_unity));
    #endregion

    #region RANK
    [SerializeField] private RankDataSO m_RankDataSO;
    [SerializeField] private PropertySet<RankType, RankDataConfig> m_RankDataConfig;

    public static DatabaseReference RankDataConfigDB 
        => RootDB.Child("Rank Data Config");

    public static RankData GetRankOfElo(int elo) 
        => RankHelper.GetRankOfElo(elo, Instance.m_RankDataConfig, Instance.m_RankDataSO);

    public static RankData GetRankOfCurrentUser()
        => RankHelper.GetRankOfElo(UserData.elo, Instance.m_RankDataConfig, Instance.m_RankDataSO);
    #endregion

    #region SAVE LOAD PATTERN
    private static async Task<T> LoadObjectAsync<T>(DatabaseReference dataRef) {
        print($"Start load {typeof(T).Name} ({Time.time})");
        string json = (await dataRef.GetValueAsync()).GetRawJsonValue() 
            ?? throw new UserNotHaveDataException();
        T obj = JsonConvert.DeserializeObject<T>(json);
        print($"Done load {typeof(T).Name} ({Time.time})");
        return obj;
    }

    private static async Task SaveObjectAsync(DatabaseReference dataRef, object data) {
        print($"Start save {data.GetType().Name} ({Time.time})");
        try {
            await dataRef.SetRawJsonValueAsync(JsonConvert.SerializeObject(data));
        } catch (Exception ex) {
            Debug.LogException(ex);
        }
        print($"Done save {data.GetType().Name} ({Time.time})");
    }
    #endregion

    #region USER DATA
    [SerializeField] private BindableProperty<UserData> m_UserData;

    public static DatabaseReference UserDB 
        => RootDB.Child("User Data");

    public static DatabaseReference CurrentUserDB 
        => UserDB.Child(AuthHelper.User.UserId);

    public static UserData UserData {
        get => Instance.m_UserData.Value;
        private set => Instance.m_UserData.Value = value;
    }

    public static UnityEvent<UserData> OnUserDataChanged
        => Instance?.m_UserData?.OnChanged;

    private static UserData GenDefaultUserDataForCurrentUser() => new() {
        id_firebase = AuthHelper.User.UserId,
        id_unity = AuthHelper.id_unity,
        name = AuthHelper.User.DisplayName,
        elo = 0,
        followed_player_id_firebase = new(),
        setting = new() {
            music = true,
            sound = true,
            vibration = true,
        }
    };

    public static async Task<UserData> LoadUserDataAsync(string id_firebase) { 
        UserData data = await LoadObjectAsync<UserData>(UserDB.Child(id_firebase));
        data.followed_player_id_firebase ??= new();
        return data;
    }

    private static async Task LoadCurrentUserDataAsync() {
        UserData data = null;

        try {
            data = await LoadObjectAsync<UserData>(CurrentUserDB);
        } catch (UserNotHaveDataException) {
            try {
                await Task.WhenAll(
                    SaveObjectAsync(CurrentUserDB, GenDefaultUserDataForCurrentUser()),
                    SaveObjectAsync(UnityToFireBaseDB.Child(AuthHelper.id_unity), AuthHelper.User.UserId));
                data = await LoadObjectAsync<UserData>(CurrentUserDB);
            } catch (Exception e) { Debug.LogException(e); }
        } catch (Exception e) { Debug.LogException(e); }

        if (data != null) {
            data.followed_player_id_firebase ??= new();
            UserData = data;
        }
    }

    public static async Task SaveCurrentUserDataAsync() {
        OnUserDataChanged.Invoke(UserData);
        await SaveObjectAsync(CurrentUserDB, UserData);
    }

    public static void ClearCachedUserData()
        => UserData = null;
    #endregion

    #region RANK DATA CONFIG
    private static async Task LoadRankDataConfigAsync() {
        try {
            Instance.m_RankDataConfig = await LoadObjectAsync<PropertySet<RankType, RankDataConfig>>(RankDataConfigDB);
        } catch (UserNotHaveDataException) {
            Instance.m_RankDataConfig = new();
        } catch (Exception e) { Debug.LogException(e); }
    }

    private static async Task SaveRankDataConfigAsync()
        => await SaveObjectAsync(RankDataConfigDB, Instance.m_RankDataConfig);
    #endregion

    public class UserNotHaveDataException : Exception { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataHelper))]
    public class DataManagerEditor : Editor {
        private SerializedProperty m_RankDataSO;
        private SerializedProperty m_RankDataConfig;
        private SerializedProperty m_UserData;
        private SerializedProperty m_SceneBoostData;

        private void OnEnable() {
            m_RankDataSO = serializedObject.FindProperty(nameof(DataHelper.m_RankDataSO));
            m_RankDataConfig = serializedObject.FindProperty(nameof(DataHelper.m_RankDataConfig));
            m_UserData = serializedObject.FindProperty(nameof(DataHelper.m_UserData));
            m_SceneBoostData = serializedObject.FindProperty(nameof(DataHelper.m_SceneBoostData));
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("CONFIG", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_RankDataSO, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JUST VIEW", EditorStyles.boldLabel);
            RenderDataField(
                m_RankDataConfig,
                new("Load", () => _ = DataHelper.LoadRankDataConfigAsync()),
                new("Save", () => _ = DataHelper.SaveRankDataConfigAsync()));
            RenderDataField(
                m_UserData,
                new("Load", () => _ = DataHelper.LoadCurrentUserDataAsync()),
                new("Save", () => _ = DataHelper.SaveCurrentUserDataAsync()));
            EditorGUILayout.PropertyField(m_SceneBoostData, true);

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderDataField(SerializedProperty data, params EditorButton[] buttons) {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PropertyField(data, true);

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, GUILayout.Width(1), GUILayout.ExpandHeight(true)), Color.gray);
            
            //if (buttons.Length != 0) {
                float maxWidthBtn = buttons.Max(button => GUI.skin.button.CalcSize(new(button.name)).x);
                EditorGUILayout.BeginVertical(GUILayout.Width(maxWidthBtn));
                foreach (var button in buttons) button.Display(maxWidthBtn);
                EditorGUILayout.EndVertical();
            //}

            EditorGUILayout.EndHorizontal();

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
        }

        private class EditorButton {
            public string name;
            public Action onClick;
            public EditorButton(string name, Action onClick) {
                this.name = name;
                this.onClick = onClick;
            }
            public void Display(float width) {
                if (GUILayout.Button(name, GUILayout.Width(width)))
                    onClick.Invoke();
            }
        }
    }
#endif
}
