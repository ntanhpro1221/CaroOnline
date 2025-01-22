using UnityEngine;

[CreateAssetMenu(fileName = "ClientDataAsset", menuName = "Data/ClientDataAsset")]
public class ClientDataAsset : ScriptableObject {
    [field: SerializeField]
    public string client_id { get; private set; }
    [field: SerializeField]
    public string client_secret { get; private set; }
}
