using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonField : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _Content;
    [SerializeField] private ClickyButton _Button;
    [SerializeField] private Image _Background;

    public ButtonField Build(CreateOption option)
        => WithContent(option.content)
        .WithCallback(option.callback)
        .WithBackgroundColor(option.backgroundColor)
        .WithForegroundColor(option.foregroundColor)
        .WithMaxTextSize(option.maxTextSize);

    public ButtonField WithContent(string content) {
        if (content == null) return this;
        _Content.text = content;
        return this;
    }

    public ButtonField WithCallback(UnityAction callback) {
        if (callback == null) return this;
        _Button.OnAfterClick.AddListener(callback);
        return this;
    }

    public ButtonField WithBackgroundColor(Color? color) {
        if (color == null) return this;
        _Background.color = color.Value;
        return this;
    }

    public ButtonField WithForegroundColor(Color? color) { 
        if (color == null) return this;
        _Content.color = color.Value;
        return this;
    }

    public ButtonField WithMaxTextSize(int? maxTextSize) { 
        if (maxTextSize == null) return this;
        _Content.fontSizeMax = maxTextSize.Value;
        return this;
    }

    public struct CreateOption {
        public string content;
        public UnityAction callback;
        public Color? backgroundColor;
        public Color? foregroundColor;
        public int? maxTextSize;
    }
}
