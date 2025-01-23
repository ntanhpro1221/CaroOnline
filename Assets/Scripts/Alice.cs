using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class Bob {

}

public class Alice : MonoBehaviour {
    [SerializeField] private PropertySet<RankType, RankDataConfig> m_RankDataConfig;
}
