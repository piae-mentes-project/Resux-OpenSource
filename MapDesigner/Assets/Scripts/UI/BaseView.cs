using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseView : MonoBehaviour
{
    #region UnityEngine

    void OnEnable()
    {
        UpdateView();
    }

    void OnDisable()
    {

    }

    #endregion

    #region Public Method

    public virtual void Initialize()
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void UpdateView()
    {

    }

    public virtual void ResetView()
    {

    }

    public virtual void ShowView()
    {
        gameObject.SetActive(true);
        ResetView();
    }

    public virtual void HideView()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
