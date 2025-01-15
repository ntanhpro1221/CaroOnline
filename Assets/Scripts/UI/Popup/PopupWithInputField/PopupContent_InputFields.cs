using UnityEngine;

[RequireComponent(typeof(BasePopup))]
public class PopupContent_InputFields : MonoBehaviour {
    [SerializeField] private GameObject _LabeledInputField;
    [SerializeField] private Transform _ContentRoot;

    public (PopupContent_InputFields, LabeledInputField) WithField(string label, string placeholder) 
        => (this, 
        Instantiate(_LabeledInputField, _ContentRoot).GetComponent<LabeledInputField>()
            .WithLabel(label)
            .WithPlaceholder(placeholder));
}
