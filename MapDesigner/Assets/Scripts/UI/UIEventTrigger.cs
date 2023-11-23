using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIEventTrigger : EventTrigger
{
    #region Delegate

    public UnityEvent<PointerEventData> onDrag;
    public UnityEvent<PointerEventData> onDragStart;
    public UnityEvent<PointerEventData> onDragEnd;
    public UnityEvent<PointerEventData> onClick;
    public UnityEvent<PointerEventData> onPointerEnter;
    public UnityEvent<PointerEventData> onPointerExit;
    public UnityEvent<PointerEventData> onPointerUp;
    public UnityEvent<PointerEventData> onPointerDown;

    #endregion

    #region override

    public override void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        onDragStart?.Invoke(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        onDragEnd?.Invoke(eventData);
    }

    public override void OnDrop(PointerEventData eventData)
    {
        
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(eventData);
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke(eventData);
    }

    public override void OnScroll(PointerEventData eventData)
    {

    }

    #endregion
}
