using UnityEngine;

public enum ETest {
    HP, 
    MP
}
public class Alice : MonoBehaviour {
    [SerializeField] private UserData raw;
    [SerializeField] private BindableProperty<UserData> wrapped;
    [SerializeField] private PropertySet<ETest, UserData> set;
}
