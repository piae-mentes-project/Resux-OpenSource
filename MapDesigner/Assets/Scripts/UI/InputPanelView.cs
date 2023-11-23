using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �½��ļ����������UI
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
    /// ��ʾ���봴������
    /// </summary>
    /// <param name="title">����</param>
    /// <param name="placeHolder">��ʾ�ı�</param>
    /// <param name="content">��ʾ�ı�</param>
    /// <param name="onOk">����֮��ִ�е��¼�</param>
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
