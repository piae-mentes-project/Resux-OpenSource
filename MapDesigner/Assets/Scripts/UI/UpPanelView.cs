using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// ����UI
/// </summary>
public class UpPanelView : BaseView
{
    #region Properties

    [Header("�ļ���ť")]

    #region FileUI or other

    [SerializeField] private Button openFileButton;
    [SerializeField] private Button saveFileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exportFileButton;
    [SerializeField] private Button toolsButton;

    #endregion

    [Header("ʱ��")]

    #region Time Control

    [SerializeField] private Slider timeSlider;
    [SerializeField] private Text timeText;
    [SerializeField] private ButtonRadio musicSpeedRadio;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private InputField timeInput;
    [SerializeField] private Button jumpTimeButton;

    #endregion

    [Header("����")]

    #region Section

    [SerializeField] private InputField sectionInput1;
    [SerializeField] private InputField sectionInput2;

    #endregion

    [Space]
    [SerializeField] private Button upScaleBtn;
    [SerializeField] private Button downScaleBtn;
    [SerializeField] private Button undoStackBtn;
    [SerializeField] private UndoStackView undoStackView;

    [SerializeField] private Camera gameCamera;

    [SerializeField] private int scaleSize = 10;
    private readonly (int min, int max) sizeRange = (540, 1080);

    private string MusicLengthText;

    #endregion

    private void Start()
    {
#if UNITY_EDITOR
        OnOpenOrCreateFile();
#else
        var view = Popup.ShowSpecialWindow<CertificationPopupView>("CertificationPopupView");
        view.Initialize(OnOpenOrCreateFile);
#endif
    }

    #region Public Method

    public override void Initialize()
    {
        EditingData.GameSceneScale = gameCamera.orthographicSize / sizeRange.min;
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        JsonConvert.DefaultSettings = () =>
        {
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return jsonSerializerSettings;
        };
        // �ļ���ť����
        settingButton.onClick.AddListener(() =>
        {
            SettingPanelView.Instance.ShowSettings();
            MusicPlayManager.Instance.PauseMusic();
        });
        openFileButton.onClick.AddListener(() =>
        {
            OnOpenOrCreateFile();
            MusicPlayManager.Instance.StopMusic();
        });
        saveFileButton.onClick.AddListener(OnSaveFile);
        exportFileButton.onClick.AddListener(OnExportMapFile);
        toolsButton.onClick.AddListener(OnOpenTools);

        // �������
        timeSlider.onValueChanged.AddListener(value =>
        {
            MusicPlayManager.Instance.OnMusicProgressChanged(value);
        });

        musicSpeedRadio.Initialize();
        musicSpeedRadio.AddAllButtonClickListener(index =>
        {
            var value = 1 - index * 0.25f;
            MusicPlayManager.Instance.OnMusicSpeedChanged(value);
        });
        musicSpeedRadio.ResetRadio();

        playButton.onClick.AddListener(MusicPlayManager.Instance.PlayMusic);
        pauseButton.onClick.AddListener(MusicPlayManager.Instance.PauseMusic);
        replayButton.onClick.AddListener(() =>
        {
            MusicPlayManager.Instance.RePlayFromStartTime();
        });
        jumpTimeButton.onClick.AddListener(() =>
        {
            var time = Tools.TransStringToInt(timeInput.text) / 1000.0f;
            var length = MusicPlayManager.Instance.MusicLength;
            MusicPlayManager.Instance.OnMusicProgressChanged(time / length);
        });

        MusicPlayManager.Instance.AddMusicPlayingListener(time =>
        {
            timeSlider.value = time / MusicPlayManager.Instance.MusicLength;
            RefreshTimeText(time);
        });

        undoStackView.Initialize();
        undoStackBtn.onClick.AddListener(() =>
        {
            if (undoStackView.isActiveAndEnabled)
            {
                undoStackView.HideView();
            }
            else
            {
                undoStackView.ShowView();
            }
        });

        upScaleBtn.onClick.AddListener(()=>{OnChangeCameraSize(1f);});
        downScaleBtn.onClick.AddListener(()=>{OnChangeCameraSize(-1f);});

        sectionInput1.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
        sectionInput2.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();

        PlayerInputManager.Instance.OnCtrlCAction += () =>
        {
            OnSectionShortcut();
            OnSectionInput();
        };

        PlayerInputManager.Instance.OnCtrlVAction += () =>
        {
            OnCopyShortcut();
        };

    }

    public override void ResetView()
    {
        MusicLengthText = Tools.TimeFormat(MusicPlayManager.Instance.MusicLength);
        RefreshTimeText(MusicPlayManager.Instance.MusicTime);
    }

    #endregion

    #region Private Method

    private void OnChangeCameraSize(float deltaValue)
    {
        var currentSize = (int)(gameCamera.orthographicSize - deltaValue * scaleSize);
        gameCamera.orthographicSize = Mathf.Clamp(currentSize, sizeRange.min, sizeRange.max);
        EditingData.GameSceneScale = gameCamera.orthographicSize / sizeRange.min;

        SceneImageRenderManager.Instance.DrawPreviewLines();
    }

    private void RefreshTimeText(float musicTime)
    {
        timeText.text = $"{Tools.TimeFormat(musicTime)}/{MusicLengthText}";
        timeInput.text = ((int)(musicTime * 1000 + 0.5f)).ToString();
    }

    private void OnOpenOrCreateFile()
    {
        var projectPopupView = Popup.ShowSpecialWindow<ProjectCreateOpenPopupView>("OpenProjectPopupView");
    }

    private void OnSaveFile()
    {
        if (EditingData.IsOpenMusic)
        {
            Debug.Log("���湤��");
            SaveProject();
            Popup.ShowMessage("����ɹ���", new Color(0.1f, 0.4f, 0.7f));
        }
    }

    private void OnExportMapFile()
    {
        if (EditingData.IsOpenMusic)
        {
            Debug.Log("���湤��");
            SaveProject();
            if (!Directory.Exists(MapDesignerSettings.ProjectOutPath))
            {
                Directory.CreateDirectory(MapDesignerSettings.ProjectOutPath);
            }
            Debug.Log("�������棬ÿ�λ��ȼ���ֵ");
            foreach (Difficulty difficulty in System.Enum.GetValues(typeof(Difficulty)))
            {
                if (!ExportMap(difficulty))
                {
                    return;
                }
            }
            Popup.ShowMessage("�����ɹ���", new Color(0.1f, 0.4f, 0.7f));
        }
    }

    private bool ExportMap(Difficulty difficulty)
    {
        var editMap = EditingData.EditingMapDic[difficulty];
        Debug.Log($"��� {difficulty} �Ŀ��ж�");

        if (!EditingData.CheckMap(editMap))
        {
            Debug.Log($"{difficulty} ���ڿ��ж�");
            return false;
        }
        Debug.Log($"���� {difficulty} �Ѷ�����");
        var map = new MusicMap(editMap);
        var mapContent = JsonConvert.SerializeObject(map);
        File.WriteAllText(FileUtils.GetMapPath(difficulty, true), mapContent);
        return true;
    }

    private void OnOpenTools()
    {
        var view = Popup.ShowSpecialWindow<ToolsPopupView>("ToolsPopupView");
    }

    private void OnSectionShortcut()
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2)
        {
            EditingData.SectionTime1 = MusicPlayManager.Instance.MusicTime*1000;
            EditingData.SectionTime2 = 0f;
            EditingData.SectionSelected1 = true;
            EditingData.SectionSelected2 = false;
            return;
        }
        if(!EditingData.SectionSelected1 && !EditingData.SectionSelected2)
        {
            EditingData.SectionTime1 = MusicPlayManager.Instance.MusicTime*1000;
            EditingData.SectionTime2 = 0f;
            EditingData.SectionSelected1 = true;
            EditingData.SectionSelected2 = false;
            return;
        }
        if(EditingData.SectionSelected1 && !EditingData.SectionSelected2)
        {
            EditingData.SectionTime1 = EditingData.SectionTime1;
            EditingData.SectionTime2 = MusicPlayManager.Instance.MusicTime*1000;
            EditingData.SectionSelected1 = true;
            EditingData.SectionSelected2 = true;
            if(EditingData.SectionTime1 >= EditingData.SectionTime2){
                Popup.ShowMessage("����ѡȡ�ڶ����ڵ�һ��֮ǰ��������ѡȡ", Color.red);
                EditingData.SectionTime1 = EditingData.SectionTime1;
                EditingData.SectionTime2 = 0f;
                EditingData.SectionSelected1 = true;
                EditingData.SectionSelected2 = false;
            }
            return;
        }
        Debug.LogError("<color=red>����ѡȡѡ���˵ڶ����㵫û��ѡ���һ���㣬զ���</color>");
    }

    private void OnSectionInput()
    {
        sectionInput1.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
        sectionInput2.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        sectionInput1.interactable = EditingData.SectionSelected1;
        sectionInput2.interactable = EditingData.SectionSelected2;
        // �����̫���ˣ��Ժ��������ټӸ�ȷ����ť��д�ֶ�����
    }

    private void OnCopyShortcut(){
        Popup.ShowSpecialWindow<CopyPastePopupView>("CopyPastePopupView");
        // ��ʱ������
    }

    #endregion

    #region static Methods

    private static async void SaveMaps()
    {
        if (EditingData.IsOpenMap)
        {
            var maps = EditingData.EditingMapDic.Values;
            foreach (var map in maps)
            {
                if (EditingData.CheckMap(map))
                {
                    await FileUtils.SaveDifficulty(map);
                }
            }

            // FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Tale]);
            // FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Romance]);
            // FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.History]);
            // FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Story]);
            // FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Revival]);
        }
    }

    public static void SaveProject()
    {
        // ���ñ���
        GlobalSettings.SaveSettings();
        MapDesignerSettings.SaveSettings();
        // ���汣��
        SaveMaps();
    }

    #endregion
}