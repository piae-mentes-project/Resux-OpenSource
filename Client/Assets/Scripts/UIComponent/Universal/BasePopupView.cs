using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// 弹窗的基类
    /// </summary>
    public class BasePopupView : MonoBehaviour
    {
        #region properties

        [SerializeField] private Button closeButton;

        public event System.Action onClose;

        #endregion

        #region Public Method

        public virtual void Initialize()
        {
            closeButton?.onClick.AddListener(Close);
        }

        public virtual void Close()
        {
            Destroy(gameObject);
            onClose?.Invoke();
        }

        #endregion

        #region Protected Method



        #endregion
    }
}
