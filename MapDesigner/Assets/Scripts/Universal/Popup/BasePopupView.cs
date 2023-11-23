using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasePopupView : MonoBehaviour
{
    #region properties

    [SerializeField] private Button closeButton;

    private UnityAction onClose;

    #endregion

    #region Public Method

    public virtual void Initialize()
    {
        closeButton.onClick.AddListener(Close);
    }

    public virtual void AddCloseListener(UnityAction onClose)
    {
        this.onClose += onClose;
    }

    #endregion

    #region Protected Method

    protected virtual void Close()
    {
        onClose?.Invoke();
        Destroy(gameObject);
    }

    #endregion

    #region Private Method



    #endregion
}
