using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleField : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _LabelTxt;
    [SerializeField] private Toggle _Toggle;

    public bool Value
        => _Toggle.isOn;
    
    public ToggleField WithLabel(string label) {
        _LabelTxt.text = label;
        return this;
    }

    public ToggleField WithLabelColor(Color color) {
        _LabelTxt.color = color;
        return this;
    }

    public ToggleField WithValue(bool value) {
        _Toggle.isOn = value;
        return this;
    }

    public ToggleField WithCallback(UnityAction<bool> callback) {
        _Toggle.onValueChanged.AddListener(callback);
        return this;
    }
}
