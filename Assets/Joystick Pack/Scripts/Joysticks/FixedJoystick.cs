using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class FixedJoystick : Joystick
{
    public bool is_dragging;
    override public void OnPointerDown(PointerEventData eventData)
    {
        is_dragging = true;
        OnDrag(eventData);
    }

    override public void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        is_dragging = false;
    }
}