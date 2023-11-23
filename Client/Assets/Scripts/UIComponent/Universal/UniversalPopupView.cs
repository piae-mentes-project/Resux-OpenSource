using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// 通用弹窗
    /// </summary>
    public class UniversalPopupView : BasePopupView
    {
        #region properties

        [SerializeField]
        private Button cancel;

        [SerializeField]
        private Button ok;

        [SerializeField]
        private Text message;

        [SerializeField]
        private Text hintTitle;

        #endregion

        public void Initialize(string message, PopupType popupType = PopupType.Normal, UnityAction onCancel = null, UnityAction onOk = null)
        {
            this.message.text = message.Localize();
            switch (popupType)
            {
                case PopupType.Normal:
                    hintTitle.gameObject.SetActive(false);
                    break;
                case PopupType.Warn:
                    hintTitle.gameObject.SetActive(true);
                    hintTitle.text = "警告";
                    break;
                case PopupType.Error:
                    break;
            }

            // 默认为关闭弹窗方法
            cancel.onClick.AddListener(Close);
            ok.onClick.AddListener(Close);

            if (onCancel == null)
            {
                cancel.gameObject.SetActive(onOk == null);
            }
            else
            {
                cancel.onClick.AddListener(onCancel);
            }

            if (onOk != null)
            {
                ok.onClick.AddListener(onOk);
            }
        }
    }
}