using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class DataHelper : Singleton<DataHelper> {
    public static DatabaseReference RootDB => FirebaseDatabase.DefaultInstance.RootReference;
    public static DatabaseReference CurrentUserDB => RootDB.Child(AuthHelper.User.UserId);

    [SerializeField] private UserData m_UserData;
    public static UserData UserData {
        get => Instance.m_UserData;
        private set => Instance.m_UserData = value;
    }

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

    public static async Task<UserData> LoadUserDataAsync(string id_firebase)
        => await LoadObjectAsync<UserData>(RootDB.Child(id_firebase));

    public static async Task LoadCurrentUserDataAsync() {
        UserData data = null;
        try {
            data = await LoadObjectAsync<UserData>(CurrentUserDB);
        } catch (UserNotHaveDataException) {
            try {
                await SaveObjectAsync(CurrentUserDB, GenDefaultUserDataForCurrentUser());
                data = await LoadObjectAsync<UserData>(CurrentUserDB);
            } catch (Exception e) { Debug.LogException(e); }
        } catch (Exception e) { Debug.LogException(e); }

        if (data != null) UserData = data;
    }

    public static async Task SaveCurrentUserDataAsync()
        => await SaveObjectAsync(CurrentUserDB, UserData);

    public static void ClearCachedUserData()
        => UserData = null;
    
    public class UserNotHaveDataException : Exception { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataHelper))]
    public class DataManagerEditor : Editor {
        private SerializedProperty m_UserData;

        private void OnEnable() {
            m_UserData = serializedObject.FindProperty(nameof(DataHelper.m_UserData));
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            RenderDataField(
                m_UserData,
                new("Load", () => _ = DataHelper.LoadCurrentUserDataAsync()),
                new("Save", () => _ = DataHelper.SaveCurrentUserDataAsync()));

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderDataField(SerializedProperty data, params EditorButton[] buttons) {
            // Label
            EditorGUILayout.BeginHorizontal();
            bool fold = EditorGUILayout.PropertyField(data, false);
            foreach (var button in buttons) button.Display();
            EditorGUILayout.EndHorizontal();

            // Child element
            EditorGUI.indentLevel++;
            if (fold) foreach (var ite in GetDirectChilds(data))
                    EditorGUILayout.PropertyField(ite);
            EditorGUI.indentLevel--;
        }

        private IEnumerable<SerializedProperty> GetDirectChilds(SerializedProperty prop) {
            if (!prop.hasVisibleChildren) yield break;
            var ite = prop.Copy(); ite.NextVisible(true);
            var end = prop.GetEndProperty();
            do {
                EditorGUILayout.PropertyField(ite);
                ite.NextVisible(false);
            } while (!SerializedProperty.EqualContents(ite, end));
        }

        private class EditorButton {
            public string name;
            public Action onClick;
            public EditorButton(string name, Action onClick) {
                this.name = name;
                this.onClick = onClick;
            }
            public void Display() {
                if (GUILayout.Button(name))
                    onClick.Invoke();
            }
        }
    }
#endif
}
