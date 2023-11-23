using System.Collections;
using System.Collections.Generic;
using Resux.Data;
using Resux.Manager;
using Resux.UI.Component.Effect;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class DspBufferPopupView : BasePopupView
    {
        #region properties

        private const int baseSize = 32;

        [SerializeField] private Text titleText;
        [SerializeField] private Text cancelButtonText;
        [SerializeField] private Button okButton;
        [SerializeField] private Text okButtonText;
        [SerializeField] private RawImage backgroundImage;

        [SerializeField] private Slider dspBufferSlider;
        [SerializeField] private Text dspBufferValueText;
        [SerializeField] private Text labelText;
        [SerializeField] private Text hintText;
        [SerializeField] private Button playButton;
        [SerializeField] private Text resetAndTestButtonText;

        private int size;
        private GamePreSetting preSetting;
        private AudioConfiguration audioConfiguration;

        #endregion

        #region Public Method

        public override void Initialize()
        {
            base.Initialize();

            okButton.onClick.AddListener(OnOk);
            dspBufferSlider.onValueChanged.AddListener(OnValueChanged);
            playButton.onClick.AddListener(() =>
            {
                audioConfiguration.dspBufferSize = size;
                AudioSettings.Reset(audioConfiguration);
                AudioPlayManager.Instance.Unload();
                AudioPlayManager.Init();
                // AudioPlayManager.Instance.PlayBGM(SoundBank.Bgm.ResultScene);
                AudioPlayManager.Instance.ReplayBGM();
            });
            // AudioPlayManager.Instance.PlayBGM(SoundBank.Bgm.ResultScene);

            // 本地化
            titleText.text = "SET_DSP_BUFFER_TITLE".Localize();
            hintText.text = "SET_DSP_BUFFER_HINT".Localize();
            resetAndTestButtonText.text = "SET_DSP_BUFFER_TEST".Localize();
            cancelButtonText.text = "SET_DSP_BUFFER_CANCEL".Localize();
            okButtonText.text = "SET_DSP_BUFFER_FINISH".Localize();

            labelText.text = "DSP Buffer".Localize();

            audioConfiguration = AudioSettings.GetConfiguration();
            dspBufferValueText.text = size.ToString();
            // 这里不用转换成float也没事
            dspBufferSlider.value = GetRealPow(size);
        }

        public void SetBackground(Texture2D bg)
        {
            backgroundImage.texture = bg;
            backgroundImage.GetComponent<ImageBlurGPU>().StartBlur(false);
        }

        public void SetSetting(GamePreSetting gamePreSetting)
        {
            preSetting = gamePreSetting;
            size = preSetting.DspBufferSize;
            dspBufferValueText.text = size.ToString();
            dspBufferSlider.value = GetRealPow(size);
        }

        public override void Close()
        {
            base.Close();
            // 因为设置界面也需要播bgm所以不停了
            // AudioPlayManager.Instance.StopBGM();
        }

        #endregion

        #region Protected Method



        #endregion

        #region Private Method

        private void OnOk()
        {
            preSetting.DspBufferSize = size;
            Close();
        }

        /// <param name="value">幂次</param>
        private void OnValueChanged(float value)
        {
            size = GetRealSize(value);
            dspBufferValueText.text = size.ToString();
        }

        /// <summary>
        /// 根据给定幂次获取真实dsp buffer
        /// </summary>
        /// <param name="pow">幂次</param>
        /// <returns></returns>
        private int GetRealSize(float pow)
        {
            return baseSize * (int)(Mathf.Pow(2, pow) + 0.5f);
        }

        /// <summary>
        /// 根据dsp buffer获取对应幂次
        /// </summary>
        /// <param name="size">dsp buffer</param>
        /// <returns></returns>
        private int GetRealPow(int size)
        {
            var multi = size / baseSize;
            var count = 0;
            // 2的0次方=1 (128 / 128)
            while (multi > 1)
            {
                multi /= 2;
                count++;
            }

            return count;
        }

        #endregion
    }
}
