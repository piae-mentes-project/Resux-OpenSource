using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

/// <summary>
/// 设置面板UI
/// </summary>
public class SettingPanelView : MonoBehaviour
{
    public static SettingPanelView Instance;

    #region Properties

    /// <summary>谱面路径输入</summary>
    [SerializeField] InputField mapPathInputField;
    /// <summary>选择路径按钮</summary>
    [SerializeField] Button selectPathButton;
    /// <summary>音乐音量条</summary>
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Text musicVolumeText;
    /// <summary>打击音量条</summary>
    [SerializeField] Slider tapAudioVolumeSlider;
    [SerializeField] Text tapAudioVolumeText;
    /// <summary>自动保存间隔条</summary>
    [SerializeField] Slider autoSaveIntervalSlider;
    [SerializeField] Text autoSaveIntervalText;
    /// <summary>自动保存数量条</summary>
    [SerializeField] Slider autoSaveCountSlider;
    [SerializeField] Text autoSaveCountText;
    /// <summary>最大预览小节数量条</summary>
    [SerializeField] Slider maxPreviewMeasureCountSlider;
    [SerializeField] Text maxPreviewMeasereCountText;
    /// <summary>打击特效大小条</summary>
    [SerializeField] Slider EffectSizeSlider;
    [SerializeField] Text EffectSizeText;
    /// <summary>网格吸附开关</summary>
    [SerializeField] Toggle posLimitSwitch;
    /// <summary>模拟游戏内预览的自动模式切换</summary>
    [SerializeField] Toggle previewModeSwitch;

    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    #region Public Method

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        Instance = this;

        // 设置初始状态
        autoSaveIntervalSlider.wholeNumbers = true;
        autoSaveIntervalSlider.maxValue = ConstData.AutoSaveIntervalRange.max;
        autoSaveIntervalSlider.minValue = ConstData.AutoSaveIntervalRange.min;
        autoSaveCountSlider.wholeNumbers = true;
        autoSaveCountSlider.maxValue = ConstData.AutoSaveCountRange.max;
        autoSaveCountSlider.minValue = ConstData.AutoSaveCountRange.min;
        maxPreviewMeasureCountSlider.wholeNumbers = true;
        maxPreviewMeasureCountSlider.maxValue = ConstData.MaxPreviewMeasureCount.max;
        maxPreviewMeasureCountSlider.minValue = ConstData.MaxPreviewMeasureCount.min;

        // 设置监听
        musicVolumeSlider.onValueChanged.AddListener(value => musicVolumeText.text = value.ToString());
        tapAudioVolumeSlider.onValueChanged.AddListener(value => tapAudioVolumeText.text = value.ToString());
        autoSaveIntervalSlider.onValueChanged.AddListener(value =>
        {
            var interval = (int) (value + 0.5f);
            autoSaveIntervalText.text = interval.ToString();
        });
        autoSaveCountSlider.onValueChanged.AddListener(value =>
        {
            var count = (int)(value + 0.5f);
            autoSaveCountText.text = count.ToString();
        });
        maxPreviewMeasureCountSlider.onValueChanged.AddListener(value =>
        {
            var count = (int)(value + 0.5f);
            maxPreviewMeasereCountText.text = count.ToString();
        });
        posLimitSwitch.onValueChanged.AddListener(isOn =>
        {
            if (isOn ^ GlobalSettings.IsPosLimitActive)
            {
                GlobalSettings.IsPosLimitActive = isOn;
            }
        });
        previewModeSwitch.onValueChanged.AddListener(isOn =>
        {
            if (isOn ^ GlobalSettings.IsPreviewAutoPlayOn)
            {
                GlobalSettings.IsPreviewAutoPlayOn = isOn;
            }
        });
        okButton.onClick.AddListener(OnOkButton);
        cancelButton.onClick.AddListener(OnCancelButton);
        selectPathButton.onClick.AddListener(OnSelectMapSavePath);
        EffectSizeSlider.onValueChanged.AddListener(value => EffectSizeText.text = value.ToString());
    }

    /// <summary>
    /// 打开设置面板
    /// </summary>
    public void ShowSettings()
    {
        gameObject.SetActive(true);
        mapPathInputField.text = GlobalSettings.MapDataPath;
        musicVolumeSlider.value = GlobalSettings.MusicVolume;
        tapAudioVolumeSlider.value = GlobalSettings.TapAudioVolume;
        // autoSaveIntervalSlider.value = Tools.NormalizeValue(GlobalSettings.AutoSaveInterval, ConstData.AutoSaveIntervalRange.min, ConstData.AutoSaveIntervalRange.max); ;
        autoSaveIntervalSlider.value = GlobalSettings.AutoSaveInterval;
        // autoSaveCountSlider.value = Tools.NormalizeValue(GlobalSettings.AutoSaveCount, ConstData.AutoSaveCountRange.min, ConstData.AutoSaveCountRange.max);
        autoSaveCountSlider.value = GlobalSettings.AutoSaveCount;
        // maxPreviewMeasureCountSlider.value = Tools.NormalizeValue(GlobalSettings.MaxPreviewMeasureCount, ConstData.MaxPreviewMeasureCount.min, ConstData.MaxPreviewMeasureCount.max);
        maxPreviewMeasureCountSlider.value = GlobalSettings.MaxPreviewMeasureCount;
        EffectSizeSlider.value = GlobalSettings.EffectSize;
        posLimitSwitch.isOn = GlobalSettings.IsPosLimitActive;
        previewModeSwitch.isOn = GlobalSettings.IsPreviewAutoPlayOn;
    }

    #endregion

    #region Private Method

    private void OnOkButton()
    {
        // 保存设置
        GlobalSettings.MapDataPath = mapPathInputField.text;
        MusicPlayManager.Instance.MusicVolume = GlobalSettings.MusicVolume = musicVolumeSlider.value;
        MusicPlayManager.Instance.EffectVolume = GlobalSettings.TapAudioVolume = tapAudioVolumeSlider.value;
        // GlobalSettings.AutoSaveInterval = Tools.Scale01ValueToRealRange(autoSaveIntervalSlider.value, ConstData.AutoSaveIntervalRange.min, ConstData.AutoSaveIntervalRange.max);
        GlobalSettings.AutoSaveInterval = autoSaveIntervalSlider.value;
        // GlobalSettings.AutoSaveCount = (int) Tools.Scale01ValueToRealRange(autoSaveCountSlider.value, ConstData.AutoSaveCountRange.min, ConstData.AutoSaveCountRange.max);
        GlobalSettings.AutoSaveCount = (int) (autoSaveCountSlider.value + 0.5f);
        // GlobalSettings.MaxPreviewMeasureCount = (int) Tools.Scale01ValueToRealRange(maxPreviewMeasureCountSlider.value, ConstData.MaxPreviewMeasureCount.min, ConstData.MaxPreviewMeasureCount.max);
        GlobalSettings.MaxPreviewMeasureCount = (int) (maxPreviewMeasureCountSlider.value + 0.5f);
        GlobalSettings.EffectSize = EffectSizeSlider.value;

        GlobalSettings.SaveSettings();

        gameObject.SetActive(false);
    }

    private void OnCancelButton()
    {
        gameObject.SetActive(false);
    }

    private void OnSelectMapSavePath()
    {
        var path = FileUtils.GetFolderPath("选择工程存储路径");
        if (!string.IsNullOrEmpty(path))
        {
            mapPathInputField.text = path;
        }
    }

    #endregion
}
