using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UI;
using UnityEngine;

public class ToggleButton : ButtonBase
{
    public ColorType onBackgroundColor;
    public ColorType onTextColor;
    public ColorType onBackgroundColorOnHover;
    public ColorType onTextColorOnHover;
    public ColorType offBackgroundColor;
    public ColorType offTextColor;
    public ColorType offBackgroundColorOnHover;
    public ColorType offTextColorOnHover;

    private readonly float _iconPositionX = 25;
    public bool IsOn { get; private set; } = true;

    private void SetValue(bool value)
    {
        IsOn = value;
        Refresh(UIManager.Instance.HoverBaseDuration);
    }
    
    public void Toggle() => SetValue(!IsOn);
    
    public override void Refresh(float animationDuration = 0f)
    {
        backgroundColor = IsOn ? onBackgroundColor : offBackgroundColor;
        textColor = IsOn ? onTextColor : offTextColor;
        backgroundColorOnHover = IsOn ? onBackgroundColorOnHover : offBackgroundColorOnHover;
        textColorOnHover = IsOn ? onTextColorOnHover : offTextColorOnHover;
        var moveCircleTo = IsOn ? new Vector3(_iconPositionX, 0, 0) : new Vector3(-_iconPositionX, 0, 0);
        UIManager.Instance.Animator.Move(icon.gameObject, moveCircleTo, UIManager.Instance.HoverBaseDuration);
        base.Refresh(animationDuration);
    }
    
}
