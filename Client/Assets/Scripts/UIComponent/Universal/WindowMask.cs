using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Resux.UI{
    public class WindowMask:MonoBehaviour{
        #region properties
            #region Scene Object
                [SerializeField] public GameObject Mask;
            #endregion
        #endregion
        #region Public Method
            public void Initialize(){
                Mask.SetActive(false);
            }
            public void Show(){
                Mask.SetActive(true);
            }
            public void Hide(){
                Mask.SetActive(false);
            }
        #endregion
    }
}