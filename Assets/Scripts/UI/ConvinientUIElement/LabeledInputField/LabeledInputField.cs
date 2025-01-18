using TMPro;
using UnityEngine;

public class LabeledInputField : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _LabelTxt;
    [SerializeField] private TMP_InputField _InputField;
    [SerializeField] private TextMeshProUGUI _PlaceholderTxt;

    public string Text 
        => _InputField.text;
    
    public LabeledInputField WithText(string text) {
        _InputField.text = text;
        return this;
    }

    public LabeledInputField WithLabel(string label) {
        _LabelTxt.text = label;
        return this;
    }

    public LabeledInputField WithPlaceholder(string placeholder) {
        _PlaceholderTxt.text = placeholder;
        return this;
    }
}
