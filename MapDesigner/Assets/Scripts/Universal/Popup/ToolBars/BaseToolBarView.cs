using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseToolBarView : MonoBehaviour
{
    #region properties

    [SerializeField] private Button okButton;

    protected Action onOkButton;

    #endregion

    public virtual void Initialize(Action onOk = null)
    {
        okButton.onClick.AddListener(OnOk);

        onOkButton = onOk;
    }

    public abstract void OnOk();
}
