using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Resux.UI
{
    /// <summary>
    /// 弹窗界面
    /// </summary>
    public class PopupView : MonoBehaviour
    {
        public static PopupView Instance => instance;

        private static PopupView instance;

        #region properties

        [SerializeField]
        private Transform PopupParent;

        private Stack<BasePopupView> popupViews;

        #endregion

        #region Resources

        private static GameObject UniversalPopupViewPrefab;
        private const string PopupUIPath = "Prefabs/UI/Popup";

        #endregion

        static PopupView()
        {

        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
            UniversalPopupViewPrefab = Resources.Load<GameObject>($"{PopupUIPath}/UniversalPopup");
            popupViews = new Stack<BasePopupView>();
        }

        /// <summary>
        /// 显示错误弹窗
        /// </summary>
        public GameObject ShowErrorWindow(string message = "未知错误", UnityAction onCancel = null, UnityAction onOk = null)
        {
            return ShowUniversalWindow(message, PopupType.Error, onCancel, onOk);
        }

        /// <summary>
        /// 显示通用弹窗
        /// </summary>
        /// <param name="onCancel"></param>
        /// <param name="onOk"></param>
        public GameObject ShowUniversalWindow(string message, PopupType popupType = PopupType.Normal, UnityAction onCancel = null, UnityAction onOk = null)
        {
            var universalPopup = Instantiate(UniversalPopupViewPrefab, PopupParent ?? transform).GetComponent<UniversalPopupView>();
            universalPopup.Initialize(message, popupType, onCancel, onOk);
            popupViews.Push(universalPopup);
            universalPopup.onClose += () => popupViews.Pop();
            return universalPopup.gameObject;
        }

        /// <summary>
        /// 显示特殊弹窗
        /// </summary>
        /// <typeparam name="T">需要返回的类型实例</typeparam>
        /// <param name="name">弹窗名</param>
        /// <returns>弹窗携带的类型实例</returns>
        public T ShowSpecialWindow<T>(string name) where T : BasePopupView
        {
            var panel = Resources.Load<GameObject>($"{PopupUIPath}/{name}");
            panel = Instantiate(panel, PopupParent ?? transform);
            var panelView = panel.GetComponent<T>();
            panelView.Initialize();
            popupViews.Push(panelView);
            panelView.onClose += () => popupViews.Pop();
            return panelView;
        }

        /// <summary>
        /// 系统的返回
        /// </summary>
        /// <returns>是否存在可返回对象</returns>
        public bool OnEscape()
        {
            BasePopupView popupView = null;
            while(popupViews.Count > 0)
            {
                popupView = popupViews.Peek();
                if (popupView == null)
                {
                    popupViews.Pop();
                }
                else
                {
                    break;
                }
            }

            if (popupView == null)
            {
                return false;
            }
            else
            {
                popupView.Close();
                return true;
            }
        }
    }
}
