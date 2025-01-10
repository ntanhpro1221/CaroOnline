using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Alice : MonoBehaviour {
    // is init
    [Command]
    public void A() {
        print(BattleConnector.Instance.IsStarted);
    }

    [Command]
    public void B() {
        print(FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length);
    }

    [Command]
    public void C() {
        print(FindObjectsByType<SimplePopupUI>(FindObjectsSortMode.None).Length);
    }
}
