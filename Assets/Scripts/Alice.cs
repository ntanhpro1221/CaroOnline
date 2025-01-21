using Unity.Netcode;
using UnityEngine;

public class Alice : MonoBehaviour {
    private void Update() {
        print(NetworkManager.Singleton.ConnectedClients.Count);
    }
}
