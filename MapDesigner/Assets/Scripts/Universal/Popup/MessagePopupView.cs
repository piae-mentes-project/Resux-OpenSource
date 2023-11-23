using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessagePopupView : BasePopupView
{
    #region properties

    [SerializeField] private Text _messageText;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _cancelButton;

    #endregion

    #region Public Message

    public void Initialize(string message, Color color, bool showCancel = false, UnityAction onOk = null, UnityAction onCancel = null)
    {
        SetMessage(message, color);
        _okButton.onClick.AddListener(Close);
        _cancelButton.onClick.AddListener(Close);
        if (onOk != null)
        {
            _okButton.onClick.AddListener(onOk);
        }

        if (onCancel != null)
        {
            _cancelButton.onClick.AddListener(onCancel);
        }
        _cancelButton.gameObject.SetActive(showCancel);
    }

    public void SetMessage(string message, Color color)
    {
        _messageText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{message}</color>";
    }

    #endregion

    #region Private Method



    #endregion
}
