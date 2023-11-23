using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Resux
{
    public class DraggableUIElement : MonoBehaviour
    {
        /// <summary>
        /// ��С�϶�ƫ������������ָ����Ϊ�϶���������Ϊ���
        /// </summary>
        [SerializeField] public float MinDragRadius = 15f;
        
        /// <summary>
        /// ��Ԫ�ر����ʱ�����¼�
        /// </summary>
        public event Action<PointerEventData> onClick;

        public event Action<PointerEventData> onBeginDrag;
        public event Action<PointerEventData> onDrag;
        public event Action<PointerEventData, Vector2> onEndDrag;

        void Awake()
        {
            var trigger = gameObject.AddComponent<DraggableEventTrigger>();
            trigger.Parent = this;
        }

        /// <summary>
        /// �������¼������
        /// </summary>
        private class DraggableEventTrigger : EventTrigger
        {
            private bool isDrag = false;
            private Vector2? startPos = null;
            public DraggableUIElement Parent;
            public override void OnPointerDown(PointerEventData eventData)
            {
                startPos = eventData.position;
                base.OnPointerDown(eventData);
            }

            public override void OnPointerUp(PointerEventData eventData)
            {
                base.OnPointerUp(eventData);
                if (!isDrag)
                {
                    Parent.onClick?.Invoke(eventData);
                }
                else
                {
                    Parent.onEndDrag?.Invoke(eventData, (eventData.position - startPos.Value).normalized);
                }

                startPos = null;
                isDrag = false;
            }

            public override void OnDrag(PointerEventData eventData)
            {
                base.OnDrag(eventData);
                var pos = eventData.position;

                if (Vector2.Distance(pos, startPos.Value) > Parent.MinDragRadius && !isDrag)
                {
                    isDrag = true;
                    // ��ʱ��������ʼ�϶�
                    Parent.onBeginDrag?.Invoke(eventData);
                    startPos = pos;
                    return;
                }

                if (isDrag) Parent.onDrag?.Invoke(eventData);
            }
        }
    }
}
