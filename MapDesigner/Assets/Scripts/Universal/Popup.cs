using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class Popup
{
    #region Prefabs

    private static GameObject messagePopupPrefab;

    private static Transform PopupParenTransform;

    private static Image PopupBackground;

    private static int popupViewCount;

    private static int PopupViewCount
    {
        get => popupViewCount;
        set
        {
            popupViewCount = value;
            RefreshBackgroundStatus();
        }
    }

    #endregion

    static Popup()
    {
        messagePopupPrefab = Resources.Load<GameObject>("Prefabs/MessagePopupView");
    }

    #region Public Method

    public static void SetPopupParent(Transform popupParenTransform)
    {
        PopupParenTransform = popupParenTransform;
        PopupBackground = PopupParenTransform.GetComponent<Image>();
    }

    public static void ShowMessage(string message, Color color, bool showCancelButton = false, UnityAction onOk = null, UnityAction onCancel = null)
    {
        var popupView = GameObject.Instantiate(messagePopupPrefab, PopupParenTransform).GetComponent<MessagePopupView>();
        popupView.Initialize(message, color, showCancelButton,
            onOk:() =>
            {
                onOk?.Invoke();
                PopupViewCount--;
            },
            onCancel: () =>
            {
                onCancel?.Invoke();
                PopupViewCount--;
            });
        PopupViewCount++;
    }

    /// <summary>
    /// 显示特殊弹窗
    /// </summary>
    /// <typeparam name="T">需要返回的类型实例</typeparam>
    /// <param name="name">弹窗名</param>
    /// <returns>弹窗携带的类型实例</returns>
    public static T ShowSpecialWindow<T>(string name) where T : BasePopupView
    {
        var panel = Resources.Load<GameObject>($"Prefabs/{name}");
        panel = GameObject.Instantiate(panel, PopupParenTransform);
        var panelView = panel.GetComponent<T>();
        panelView.Initialize();
        panelView.AddCloseListener((() => PopupViewCount--));
        PopupViewCount++;
        return panelView;
    }

    #endregion

    #region Private Method

    private static void RefreshBackgroundStatus()
    {
        PopupBackground.enabled = PopupViewCount > 0;
    }

    #endregion
}
