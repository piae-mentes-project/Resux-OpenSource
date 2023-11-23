using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UndoCommandDescriptionLine : MonoBehaviour
{
    #region properties

    [SerializeField] private Text descriptionText;

    #endregion

    #region Public Method

    public void SetDescription(string description)
    {
        descriptionText.text = description;
    }

    #endregion
}
