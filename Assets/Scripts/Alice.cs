using UnityEngine;

public class Alice : MonoBehaviour {
    private void Awake() {
        SelectableBoard.Instance.OnCellSelected.AddListener(MarkHelper.Instance.Mark_O);
    }
}
