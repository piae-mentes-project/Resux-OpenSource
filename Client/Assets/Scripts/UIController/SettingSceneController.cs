using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Resux.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Resux.UI;
using Resux.Data;
using Resux.GamePlay;
using Resux.LevelData;
using Resux.Manager;

namespace Resux.UI.Manager
{
    public class SettingSceneController : MonoBehaviour
    {
        #region properties

        private static GameScene ThisScene => GameScene.SettingScene;

        #region SceneObject

        [SerializeField] private Text titleText;
        [Space]
        [SerializeField] private Button backAndSaveButton;
        [SerializeField] private Text saveButtonLabel;
        [SerializeField] private Button dataSceneEntry;
        [SerializeField] private Text dataSceneText;
        [SerializeField] private Button backAndUnsaveButton;
        [SerializeField] private Text unsaveButtonLabel;
        [Space]
        [SerializeField] private Toggle audioSettingToggle;
        [SerializeField] private Toggle otherSettingToggle;
        // 还不知道是啥
        [SerializeField] private Toggle localSettingToggle;

        #region Audio Setting

        [Space, Header("音频设置")]
        [SerializeField]
        private GameObject audioSettingArea;
        [Space]
        [SerializeField] private Button autoDelayButton;
        [SerializeField] private Text autoDelayButtonLabel;
        [SerializeField] private Button dspBufferButton;
        [SerializeField] private Text dspBufferButtonLabel;

        [Space]
        [SerializeField] Text mainAudioVolumeValueText;
        [SerializeField] Text mainAudioVolumeLabelText;
        [SerializeField] Slider mainAudioVolumeSlider;
        [SerializeField] Button increaseMainAudioVolumeBtn;
        [SerializeField] Button decreaseMainAudioVolumeBtn;
        [Space]
        [SerializeField] Text musicVolumeValueText;
        [SerializeField] Text musicVolumeLabelText;
        [SerializeField] Slider musicVolumeSlider;
        [SerializeField] Button increaseMusicVolumeBtn;
        [SerializeField] Button decreaseMusicVolumeBtn;
        [Space]
        [SerializeField] Text effectAudioVolumeValueText;
        [SerializeField] Text effectAudioVolumeLabelText;
        [SerializeField] Slider effectAudioVolumeSlider;
        [SerializeField] Button increaseEffectAudioVolumeBtn;
        [SerializeField] Button decreaseEffectAudioVolumeBtn;
        [Space]
        [SerializeField] Text tapEffectAudioVolumeValueText;
        [SerializeField] Text tapEffectAudioVolumeLabelText;
        [SerializeField] Slider tapEffectAudioVolumeSlider;
        [SerializeField] Button increaseTapEffectAudioVolumeBtn;
        [SerializeField] Button decreaseTapEffectAudioVolumeBtn;
        [Space]
        [SerializeField] Text audioDelayValueText;
        [SerializeField] Text audioDelayLabelText;
        [SerializeField] Slider audioDelaySlider;
        [SerializeField] Button increaseAudioDelayBtn;
        [SerializeField] Button decreaseAudioDelayBtn;
        [Space]
        [SerializeField] Text delayHintText;

        #endregion

        #region Other Setting

        [Space, Header("其他设置")]
        [SerializeField]
        private GameObject otherSettingArea;
        [Space]
        [SerializeField] Text effectSizeNumText;
        [SerializeField] Text effectSizeLabelText;
        [SerializeField] Slider effectSizeSlider;
        [SerializeField] Button increaseEffectSizeBtn;
        [SerializeField] Button decreaseEffectSizeBtn;
        [Space]
        [SerializeField] Text noteSizeNumText;
        [SerializeField] Text noteSizeLabelText;
        [SerializeField] Slider noteSizeSlider;
        [SerializeField] Button increaseNoteSizeBtn;
        [SerializeField] Button decreaseNoteSizeBtn;
        [Space]
        [SerializeField] Text coverBrightnessText;
        [SerializeField] Text coverBrightnessLabelText;
        [SerializeField] Slider coverBrightnessSlider;
        [SerializeField] Button increaseCoverBrightnessBtn;
        [SerializeField] Button decreaseCoverBrightnessBtn;
        [Space]
        [SerializeField] Text coverBlurRateText;
        [SerializeField] Text coverBlurRateLabelText;
        [SerializeField] Slider coverBlurRateSlider;
        [SerializeField] Button increaseCoverBlurRateBtn;
        [SerializeField] Button decreaseCoverBlurRateBtn;

        #endregion

        #region Local Setting

        [Space, Header("本地不知道啥设置")]
        [SerializeField]
        private GameObject localSettingArea;
        [Space]
        [SerializeField] private Button tutorialEnterButton;
        [SerializeField] Text tutorialEnterButtonLabelText;

        #endregion

        [Space]
        [SerializeField] Text languageLabel;
        [SerializeField] Dropdown languageSelections;

        private GamePreSetting preSetting;

        #endregion

        #endregion

        #region UnityEngine

        void Start()
        {
            preSetting = new GamePreSetting(PlayerGameSettings.Setting);

            backAndSaveButton.onClick.AddListener(OnSaveButtonClicked);
            backAndSaveButton.onClick.AddListener(GlobalStaticUIManager.Instance.BackToPrevScene);
            backAndUnsaveButton.onClick.AddListener(OnUnsaveButtonClicked);
            backAndUnsaveButton.onClick.AddListener(GlobalStaticUIManager.Instance.BackToPrevScene);

            dataSceneEntry.onClick.AddListener(OnDataSceneClick);

            audioSettingToggle.onValueChanged.AddListener(isOn => ChangePanel());
            otherSettingToggle.onValueChanged.AddListener(isOn => ChangePanel());
            localSettingToggle.onValueChanged.AddListener(isOn => ChangePanel());

            autoDelayButton.onClick.AddListener(OnAutoDelayButtonClicked);
            dspBufferButton.onClick.AddListener(OnDspBufferButtonClick);
            tutorialEnterButton.onClick.AddListener(OnEnterTutorial);

            var languageList = new List<string>();
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                languageList.Add(language.GetLanguageStr());
            }

            languageSelections.AddOptions(languageList);
            languageSelections?.onValueChanged.AddListener(value =>
            {
                // 把语言的多语言key为Language
                Logger.Log($"language select value: {value}");
                Language selectLanguage = (Language)value;
                preSetting.Language = selectLanguage;
            });
            languageSelections.value = (int)preSetting.Language;

            DelaySliderUpdate();

            // load settings
            SetupSliderBinding(() => preSetting.Offset,
                (v) =>
                {
                    preSetting.Offset = Convert.ToInt32(v);
                    delayHintText.gameObject.SetActive(Mathf.Abs(preSetting.Offset) >= 80);
                },
                audioDelaySlider, audioDelayValueText,
                formatFunc: v =>
                {
                    int value = (int)((float)v + 0.5f);
                    var prefix = value > 0 ? "+" : "";
                    return $"{prefix}{value} ms";
                }
                , editButtons: new Button[] {decreaseAudioDelayBtn, increaseAudioDelayBtn},
                updateFunc: out updateOffsetValue);
            SetupSliderBinding(() => preSetting.EffectVolume,
                (v) =>
                {
                    preSetting.EffectVolume = (int)(v + 0.5f);
                    var volume = preSetting.MainAudioVolume * preSetting.EffectVolume / 100f;
                    AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.ChangeMusic, volume);
                },
                effectAudioVolumeSlider, effectAudioVolumeValueText,
                formatFunc: v => $"{v} %",
                editButtons: new Button[] {decreaseEffectAudioVolumeBtn, increaseEffectAudioVolumeBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.EffectLoudness,
                (v) =>
                {
                    preSetting.EffectLoudness = (int)(v + 0.5f);
                    var volume = preSetting.MainAudioVolume * preSetting.EffectLoudness / 100f;
                    AudioPlayManager.Instance.PlayNoteEffect(JudgeType.Tap.ToString(), volume);
                },
                tapEffectAudioVolumeSlider, tapEffectAudioVolumeValueText,
                formatFunc: v => $"{v} %",
                editButtons: new Button[] {decreaseTapEffectAudioVolumeBtn, increaseTapEffectAudioVolumeBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.EffectSize,
                (v) => preSetting.EffectSize = v,
                effectSizeSlider, effectSizeNumText,
                formatFunc: v => string.Format("{0:0.0}", v), step: 0.1f
                , editButtons: new Button[] {decreaseEffectSizeBtn, increaseEffectSizeBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.NoteSize,
                (v) => preSetting.NoteSize = v,
                noteSizeSlider, noteSizeNumText,
                formatFunc: v => string.Format("{0:0.0}", v), step: 0.1f
                , editButtons: new Button[] {decreaseNoteSizeBtn, increaseNoteSizeBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.CoverBrightness,
                (v) => preSetting.CoverBrightness = v,
                coverBrightnessSlider, coverBrightnessText,
                formatFunc: v => string.Format("{0:0%}", v), step: 0.1f
                , editButtons: new Button[] {decreaseCoverBrightnessBtn, increaseCoverBrightnessBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.CoverBlurRate,
                (v) => preSetting.CoverBlurRate = v,
                coverBlurRateSlider, coverBlurRateText,
                formatFunc: v => string.Format("{0:0%}", v), step: 0.1f
                , editButtons: new Button[] {decreaseCoverBlurRateBtn, increaseCoverBlurRateBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.MainAudioVolume,
                (v) =>
                {
                    preSetting.MainAudioVolume = v;
                    AudioPlayManager.Instance.SetBGMVolume(preSetting.MainAudioVolume * preSetting.MusicVolume);
                },
                mainAudioVolumeSlider, mainAudioVolumeValueText,
                formatFunc: v => string.Format("{0:0%}", v), step: 0.01f
                , editButtons: new Button[] {decreaseMainAudioVolumeBtn, increaseMainAudioVolumeBtn},
                updateFunc: out _);
            SetupSliderBinding(() => preSetting.MusicVolume,
                (v) =>
                {
                    preSetting.MusicVolume = v;
                    AudioPlayManager.Instance.SetBGMVolume(preSetting.MainAudioVolume * preSetting.MusicVolume);
                },
                musicVolumeSlider, musicVolumeValueText,
                formatFunc: v => string.Format("{0:0%}", v), step: 0.01f
                , editButtons: new Button[] {decreaseMusicVolumeBtn, increaseMusicVolumeBtn},
                updateFunc: out _);
        }

        private bool lastIsUsingBluetoothHeadset = false;
        private Action updateOffsetValue;
        void Update()
        {
            if (PlayerGameSettings.Setting.IsUsingBluetoothHeadset != lastIsUsingBluetoothHeadset)
            {
                lastIsUsingBluetoothHeadset = PlayerGameSettings.Setting.IsUsingBluetoothHeadset;
                preSetting.IsUsingBluetoothHeadset = PlayerGameSettings.Setting.IsUsingBluetoothHeadset;
                updateOffsetValue?.Invoke();
            }
        }

        #endregion

        #region Public Method

        public void DelaySliderUpdate()
        {
            var initVal = preSetting.Offset;
            var prefix = initVal > 0 ? "+" : "";
            audioDelayValueText.text = $"{prefix}{initVal} ms";
            audioDelaySlider.value = initVal;
            delayHintText.gameObject.SetActive(Mathf.Abs(preSetting.Offset) >= 80);
        }

        #endregion

        #region Private Method

        void SetupSliderBinding(Func<float> getter, Action<float> setter, Slider slider, Text valueView, out Action updateFunc, Button[] editButtons = null, Func<object, string> formatFunc = null, float step = 1)
        {
            var updateLock = false;

            void UpdateValue()
            {
                var initVal = getter();
                valueView.text = formatFunc == null ? initVal.ToString() : formatFunc(initVal);
                if (updateLock) return;
                slider.value = initVal;
            }

            void UpdateButtons()
            {
                if (editButtons != null)
                {
                    editButtons[0].interactable = Mathf.Abs(slider.minValue - slider.value) > 1e-5;
                    editButtons[1].interactable = Mathf.Abs(slider.maxValue - slider.value) > 1e-5;
                }
            }

            UpdateValue();
            UpdateButtons();
            slider.onValueChanged.AddListener((val) =>
            {
                setter(val);
                updateLock = true;
                UpdateValue();
                updateLock = false;
                UpdateButtons();
            });
            if (editButtons != null)
            {
                void IncreaseUpdate(bool increase)
                {
                    var val = getter() + (step) * (increase ? 1 : -1);
                    if (val > slider.maxValue || val < slider.minValue) return;
                    setter(val);
                    UpdateValue();
                }

                editButtons[0].onClick.AddListener(() => IncreaseUpdate(false));
                editButtons[1].onClick.AddListener(() => IncreaseUpdate(true));
            }

            updateFunc = UpdateValue;
        }

        private void OnSaveButtonClicked()
        {
            Logger.Log("Save Button Clicked");
            PlayerGameSettings.Setting = preSetting;
            PlayerGameSettings.SaveGamePreSetting();
            AudioPlayManager.Instance.RefreshVolumeSettings();
        }

        private void OnUnsaveButtonClicked()
        {
            var audioConfiguration = AudioSettings.GetConfiguration();
            AudioPlayManager.Instance.RefreshVolumeSettings();
            if (audioConfiguration.dspBufferSize != PlayerGameSettings.Setting.DspBufferSize)
            {
                var time = AudioPlayManager.Instance.GetBGMTime();
                audioConfiguration.dspBufferSize = PlayerGameSettings.Setting.DspBufferSize;
                AudioSettings.Reset(audioConfiguration);
                AudioPlayManager.Instance.ReplayBGM(time);
            }
        }

        private void OnAutoDelayButtonClicked()
        {
            Logger.Log("Auto Delay Button Clicked");
            CoroutineUtils.RunWhenFrameEnd(() =>
            {
                var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
                var autoDelayPanel = PopupView.Instance.ShowSpecialWindow<AutoDelayPanel>("autoDelayPanelWholeContainer");
                autoDelayPanel.AddOkButtonListener(DelaySliderUpdate);
                autoDelayPanel.SetBackground(screenShot);
                autoDelayPanel.SetSetting(preSetting);
            });
        }
        private void OnDataSceneClick()
        {
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
        }
        private void OnDspBufferButtonClick()
        {
            CoroutineUtils.RunWhenFrameEnd(() =>
            {
                var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
                var dspBufferPopupView = PopupView.Instance.ShowSpecialWindow<DspBufferPopupView>("DspBufferPopupView");
                dspBufferPopupView.SetBackground(screenShot);
                dspBufferPopupView.SetSetting(preSetting);
            });
        }

        private void ChangePanel()
        {
            var audioIsOn = audioSettingToggle.isOn;
            var localIsOn = localSettingToggle.isOn;
            var otherIsOn = otherSettingToggle.isOn;
            audioSettingArea.SetActive(audioIsOn);
            otherSettingArea.SetActive(otherIsOn);
            localSettingArea.SetActive(localIsOn);
        }

        private void OnEnterTutorial()
        {
            TutorialManager.EnterTutorialPlay();
        }

        #endregion
    }
}
