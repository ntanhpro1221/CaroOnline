using System;
using System.Collections.Generic;

[Serializable]
public class UserData {
    public string id_firebase;
    public string id_unity;
    public string name;
    public int elo;
    public List<string> followed_player_id_firebase;
    public UserSettingData setting; 
}
