using System.Collections;
using UnityEngine;
using System;

namespace Resux.UI
{
    public abstract class AbstractRecycleListItem<T> : MonoBehaviour, IRecycleListItem<T>
    {
        #region properties

        private Action OnEnterAction;
        private Action OnExitAction;
        private Action OnUpExit;
        private Action OnDownExit;
        protected Action<T> EventAction;
        /// <summary>当前格的索引</summary>
        protected int index;
        /// <summary>当前格的数据</summary>
        protected T data;
        /// <summary>务必以左上角为锚点</summary>
        [HideInInspector] public RectTransform rectTransform;
        /// <summary>显示的上下区域（数值为距离顶部的距离）</summary>
        protected Vector2 yArea;

        #endregion

        public virtual void Initialize()
        {
            rectTransform = GetComponent<RectTransform>();
            yArea = Vector2.zero;
            UpdateYArea();
        }

        public virtual void AddOnEnterListener(Action action)
        {
            OnEnterAction += action;
        }
        public virtual void AddOnExitListener(Action action)
        {
            OnExitAction += action;
        }

        public void AddOnUpExitListener(Action action)
        {
            OnUpExit += action;
        }

        public void AddOnDownExitListener(Action action)
        {
            OnDownExit += action;
        }

        public void AddEventListener(Action<T> listener)
        {
            EventAction += listener;
        }

        public virtual void OnEnter()
        {
            Show();
            OnEnterAction?.Invoke();
        }
        public virtual void OnExit()
        {
            Hide();
            OnExitAction?.Invoke();
        }

        public virtual void UpdateData(T data, int index)
        {
            this.data = data;
            this.index = index;
        }

        public virtual void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public virtual void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public virtual float GetHeight()
        {
            return rectTransform.rect.height;
        }

        public virtual float GetWidth()
        {
            return rectTransform.rect.width;
        }

        public virtual void OnScroll(Vector2 viewArea)
        {
            // Debug.Log($"<color=green>index: {index}  [yArea: {yArea}] vs [viewArea: {viewArea}]</color>");
            if (yArea.x > viewArea.y)
            {
                // Debug.Log("超出下界");
                // 向下越界
                OnDownExit?.Invoke();
                OnExit();
            }
            else if (yArea.y < viewArea.x)
            {
                // Debug.Log("超出上界");
                // 向上越界
                OnUpExit?.Invoke();
                OnExit();
            }
        }

        public virtual T GetData()
        {
            return data;
        }

        public virtual void UpdatePositon(int index, int padding, int space)
        {
            var height = GetHeight();
            var width = GetWidth();
            var pivot = rectTransform.pivot;
            var yFromTop = (height + padding) * (index + space);
            var yPos = yFromTop + (1 - pivot.y) * height + padding / 2f;
            var xPos = pivot.x * width;
            rectTransform.anchoredPosition = new Vector2(xPos, -yPos);
            UpdateYArea();
        }

        public virtual void UpdateYArea()
        {
            var sizeY = -rectTransform.anchoredPosition.y;
            var pivotY = rectTransform.pivot.y;
            var height = GetHeight();
            yArea.x = sizeY - (1 - pivotY) * height;
            yArea.y = yArea.x + height;
        }
    }
}