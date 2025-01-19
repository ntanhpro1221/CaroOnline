using UnityEngine;

public class Alice : MonoBehaviour {
    public void LoadAlice() { LoadSceneHelper.LoadScene("Alice"); } 
    public void LoadAliceAsync() { LoadSceneHelper.LoadScene("Alice", LoadSceneHelper.LoadStyle.Image); }
    public void LoadBob() { LoadSceneHelper.LoadScene("Bob"); }
    public void LoadBobAsync() { LoadSceneHelper.LoadScene("Bob", LoadSceneHelper.LoadStyle.Image); }
}
