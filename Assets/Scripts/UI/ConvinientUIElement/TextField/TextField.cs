using TMPro;
using UnityEngine;

public class TextField : MonoBehaviour { 
    [SerializeField] private TextMeshProUGUI _Content;

    public TextField WithLabel(string content) {
        _Content.text = content;
        return this;
    }

    public TextField WithColor(Color color) {
        _Content.color = color;
        return this;
    }

    public TextField WithAlignment(TextAlignmentOptions alignment) {
        _Content.alignment = alignment;
        return this;
    }
}
