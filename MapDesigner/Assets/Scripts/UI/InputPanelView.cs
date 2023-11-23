using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新建文件的输入面板UI
/// </summary>
public class InputPanelView : BaseView
{
    public static InputPanelView Instance;

    #region Properties

    [SerializeField] Text title;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;
    [SerializeField] InputField inputContent;

    private Action<string> onOk;

    #endregion

    private void Awake()
    {
        
    }

    #region Public Method

    public override void Initialize()
    {
        Instance = this;

        okButton.onClick.AddListener(OnClose);
        cancelButton.onClick.AddListener(OnClose);
    }

    /// <summary>
    /// 显示输入创建界面
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="placeHolder">提示文本</param>
    /// <param name="content">显示文本</param>
    /// <param name="onOk">输入之后执行的事件</param>
    public void ShowInputPanel(string title, string placeHolder, string content, Action<string> onOk)
    {
        gameObject.SetActive(true);
        this.title.text = title;
        inputContent.placeholder.GetComponent<Text>().text = placeHolder;
        inputContent.text = content;
        this.onOk = onOk;
    }

    #endregion

    #region Private Method

    private void OnClose()
    {
        gameObject.SetActive(false);
        onOk?.Invoke(inputContent.text);
    }

    #endregion
}
