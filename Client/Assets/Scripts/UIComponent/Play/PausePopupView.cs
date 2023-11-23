using System.Collections;
using System.Collections.Generic;
using Resux.Data;
using Resux.Manager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Resux.UI
{
    public class PausePopupView : BasePopupView
    {
        #region properties

        private UnityAction OnExit;
        private UnityAction OnContinue;
        private UnityAction OnRestart;

        #region Scene Object

        [SerializeField] private Text Title;

        [SerializeField] private Button ExitButton;
        [SerializeField] private Button ContinueButton;
        [SerializeField] private Button RestartButton;

        [SerializeField] private Component.Effect.ImageBlurGPU imageBlurGPU;

        #endregion

        #endregion

        #region Public Method

        public void Initialize(UnityAction onExit, UnityAction onContinue, UnityAction onRestart)
        {
            OnExit += onExit;
            OnContinue += onContinue;
            OnRestart += onRestart;

            ExitButton.onClick.AddListener(OnExit);
            ContinueButton.onClick.AddListener(OnContinue);
            RestartButton.onClick.AddListener(OnRestart);

            // 教程下不允许退出和重来
            ExitButton.interactable = RestartButton.interactable = !AccountManager.IsEnableTutorial;
        }

        public void ShowBlurBackground(Texture2D background)
        {
            imageBlurGPU.SetTexture(background);
            imageBlurGPU.ResetBlur();
            imageBlurGPU.StartBlur(true);
        }

        #endregion
    }
}
