using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Resux
{
    public class ExtendEventTrigger : EventTrigger
    {
        #region Delegate

        #region Pointer

        public event Action<PointerEventData> onClick;
        public event Action<PointerEventData> onPointerEnter;
        public event Action<PointerEventData> onPointerExit;
        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> onPointerUp;

        #endregion

        #region Drag

        public event Action<PointerEventData> onBeginDrag;
        public event Action<PointerEventData> onDrag;
        /// <summary>第二个参数是begin -> end的方向</summary>
        public event Action<PointerEventData, Vector2> onEndDrag;

        #endregion

        #endregion

        #region extra properties

        private Vector2? lastPos;

        #endregion

        #region Override Method

        #region Pointer

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            onClick?.Invoke(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            lastPos = eventData.position;
            base.OnPointerDown(eventData);
            onPointerDown?.Invoke(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            onPointerUp?.Invoke(eventData);
            lastPos = null;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            lastPos = eventData.position;
            base.OnPointerEnter(eventData);
            onPointerEnter?.Invoke(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onPointerExit?.Invoke(eventData);
            lastPos = null;
        }

        #endregion

        #region Drag

        public override void OnBeginDrag(PointerEventData eventData)
        {
            lastPos = eventData.position;
            base.OnBeginDrag(eventData);
            onBeginDrag?.Invoke(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            onDrag?.Invoke(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if(!lastPos.HasValue) Logger.Log("Warning: lastPos is Null");
            onEndDrag?.Invoke(eventData, (eventData.position - lastPos.Value).normalized);
            lastPos = null;
        }

        #endregion

        #endregion
    }
}