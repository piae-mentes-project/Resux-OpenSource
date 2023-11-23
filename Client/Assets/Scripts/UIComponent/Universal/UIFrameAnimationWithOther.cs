using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.UI
{
    [RequireComponent(typeof(UIFrameAnimation))]
    public class UIFrameAnimationWithOther : MonoBehaviour
    {
        #region properties

        private UIFrameAnimation selfAnimation;
        [SerializeField] private UIFrameAnimation otherAnimation;
        [SerializeField] private int startFrame;

        #endregion

        void Start()
        {
            selfAnimation = GetComponent<UIFrameAnimation>();
            otherAnimation.AddFrameAction(index => selfAnimation.ShowWhen(startFrame, index));
        }

        void Update()
        {

        }
    }
}
