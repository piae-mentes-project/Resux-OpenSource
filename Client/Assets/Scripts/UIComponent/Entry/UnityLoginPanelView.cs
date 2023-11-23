using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// Unity用的登录窗口
    /// </summary>
    public class UnityLoginPanelView : BasePopupView
    {
        #region properties

        [SerializeField] private InputField accountNameInput;
        [SerializeField] private Button okButton;

        #endregion

        #region Public Method

        public void Initialize(UnityAction<string> onOk)
        {
            accountNameInput.text = UserLocalSettings.GetString(Data.ConstConfigs.LocalKey.UnityLoginAccount, "");
            okButton?.onClick.AddListener(() =>
            {
                onOk?.Invoke(accountNameInput.text);
                Close();
            });
        }

        #endregion
    }
}
