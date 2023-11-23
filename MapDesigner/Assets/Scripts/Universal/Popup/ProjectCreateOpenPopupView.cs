using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProjectCreateOpenPopupView : BasePopupView
{
    private static GameObject ProjectInfoPrefab;

    #region properties

    [Header("�򿪹���")]
    [SerializeField] private RectTransform projectInfoContent;

    [Header("��������")]
    [SerializeField] private InputField filePathInput;
    [SerializeField] private Button selectFileButton;
    [SerializeField] private InputField projectNameInput;
    [SerializeField] private Button createProjectButton;

    private MapDesignerSettings.MapDesignerProjectSetting projectSetting;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        base.Initialize();
        if (ProjectInfoPrefab == null)
        {
            ProjectInfoPrefab = Resources.Load<GameObject>("Prefabs/ProjectInfoButton");
        }
        projectSetting = new MapDesignerSettings.MapDesignerProjectSetting();

        selectFileButton.onClick.AddListener(OnSelectFile);
        createProjectButton.onClick.AddListener(OnCreateProject);

        var directories = Directory.GetDirectories(GlobalSettings.MapDataPath);
        foreach (var directory in directories)
        {
            Debug.Log($"dir: {directory}");
            // �����ļ��в���
            if (directory.EndsWith("_out"))
            {
                continue;
            }

            try
            {
                var projectSettings = MapDesignerSettings.LoadProjectSettings($"{directory}/{MapDesignerSettings.FileName}");
                var view = Instantiate(ProjectInfoPrefab, projectInfoContent).GetComponent<ProjectInfoView>();
                view.Initialize(projectSettings, OnOpenProject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{e.Data}: {e.Message} : \n Base Exception: {e.GetBaseException()}\n StackTrace: \n {e.StackTrace}");
                continue;
            }
        }
    }

    #endregion

    #region Private Method

    private void OnSelectFile()
    {
        string tPath = FileUtils.GetMusicFilePath(new string[] { "ogg", "mp3", "wav" }, out var ext, out var fileName);
        filePathInput.text = tPath;
        projectNameInput.text = fileName.Substring(0, fileName.Length - ext.Length - 1);
        projectSetting.Ext = ext;
        projectSetting.BpmList.Add((new Beat(0), 120));
    }

    private void OnCreateProject()
    {
        var musicPath = filePathInput.text;
        if (string.IsNullOrEmpty(musicPath))
        {
            Popup.ShowMessage("������д���ݣ�", Color.red);
        }

        var projectName = projectNameInput.text;
        if (string.IsNullOrEmpty(projectName))
        {
            Popup.ShowMessage("����������/��Ŀ����", Color.red);
        }

        MapDesignerSettings.LoadSettings(projectSetting);
        var ext = projectSetting.Ext;
        var path = $"{GlobalSettings.MapDataPath}/{projectName}";
        var outpath = $"{GlobalSettings.MapDataPath}/{projectName}_out";
        MapDesignerSettings.ProjectPath = path;
        MapDesignerSettings.ProjectOutPath = outpath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.Copy(musicPath, $"{path}/audio.{ext}", true);
        // ��Ҫ���������Ѷȵ��ף�������������̵Ļ�������
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Tale]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Romance]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.History]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Story]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Revival]);
        EditingData.CurrentEditingMap = EditingData.EditingMapDic[Difficulty.Tale];
        // ������Ŀ���ò�����
        MapDesignerSettings.BpmList.Add((new Beat(0), 120));
        MapDesignerSettings.MusicName = projectName;
        MapDesignerSettings.SaveSettings();

        EditingData.IsOpenMusic = MusicPlayManager.Instance.LoadMusic(ext);
        EditingData.IsOpenMap = true;
        EditingData.SectionTime1 = 0f; EditingData.SectionSelected1 = false;
        EditingData.SectionTime2 = 0f; EditingData.SectionSelected2 = false;
        if (EditingData.CanEditing)
        {
            MainUIManager.Instance.ResetView();
            Popup.ShowMessage("�����ɹ���", Color.green);
            Close();
        }
        else
        {
            Popup.ShowMessage("������ʧ�ܣ�", Color.red);
        }
    }

    private void OnOpenProject(MapDesignerSettings.MapDesignerProjectSetting setting)
    {
        // ��Ϊ�±�load���õ� ProjectPath �� ProjectOutPath ����·���������ȸ�ֵ
        var musicName = setting.MusicName;
        MapDesignerSettings.LoadSettings(setting);
        var path = $"{GlobalSettings.MapDataPath}/{musicName}";
        var outpath = $"{GlobalSettings.MapDataPath}/{musicName}_out";
        MapDesignerSettings.ProjectPath = path;
        MapDesignerSettings.ProjectOutPath = outpath;

        if (File.Exists($"{path}/audio.{MapDesignerSettings.Ext}"))
        {
            // �򿪻򴴽������Ѷȵ���
            EditingData.EditingMapDic[Difficulty.Tale] = FileUtils.LoadDifficulty(Difficulty.Tale);
            EditingData.EditingMapDic[Difficulty.Romance] = FileUtils.LoadDifficulty(Difficulty.Romance);
            EditingData.EditingMapDic[Difficulty.History] = FileUtils.LoadDifficulty(Difficulty.History);
            EditingData.EditingMapDic[Difficulty.Story] = FileUtils.LoadDifficulty(Difficulty.Story);
            EditingData.EditingMapDic[Difficulty.Revival] = FileUtils.LoadDifficulty(Difficulty.Revival);
            Debug.Log($"current difficulty: {EditingData.CurrentMapDifficulty}");
            EditingData.CurrentMapDifficulty = Difficulty.Tale;

            EditingData.IsOpenMusic = MusicPlayManager.Instance.LoadMusic(MapDesignerSettings.Ext);
            EditingData.IsOpenMap = true;
            if (EditingData.CanEditing)
            {
                MainUIManager.Instance.ResetView();
                GamePreviewManager.Instance.InitJudgeNoteQueue();
                AutoBackupManager.Instance.CheckBackups();
                Popup.ShowMessage("�򿪳ɹ���", Color.green);
            }
            else
            {
                Popup.ShowMessage("������ʧ�ܣ�", Color.red);
            }
        }
        else
        {
            Popup.ShowMessage($"�������Ӧ·��Ŀǰ�ǣ�{GlobalSettings.MapDataPath}  Ҫ�򿪵���Ƶ��./{musicName}/audio.{setting.Ext}" +
                              $"\n����ʵ��·��������·���Ƿ�������Լ�Ҫ�򿪵Ĺ����ļ����µ�ProjectSetting.json��MusicName�Ƿ����ļ���������ͬ", Color.yellow);
        }

        Close();
    }

    #endregion
}
