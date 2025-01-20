using System;

[Serializable]
public class SceneBoostData {
    public Battle battle = new();

    [Serializable]
    public class Battle {
        public BattleMode battleMode = BattleMode.Player_Player;
    }
}
