﻿using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickyButton : Button {
    public UnityEvent OnAfterClick { get; } = new();

    public override void OnPointerDown(PointerEventData eventData) {
        base.OnPointerDown(eventData);
        SoundHelper.Play(SoundType.PointerDown);
        transform.DOScale(0.95f, 0.06f);
    }

    public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);
        SoundHelper.Play(SoundType.PointerUp);
        transform.DOScale(1, 0.1f)
            .OnComplete(() => OnAfterClick.Invoke());
    }
}
