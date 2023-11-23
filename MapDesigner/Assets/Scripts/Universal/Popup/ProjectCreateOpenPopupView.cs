using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProjectCreateOpenPopupView : BasePopupView
{
    private static GameObject ProjectInfoPrefab;

    #region properties

    [Header("打开工程")]
    [SerializeField] private RectTransform projectInfoContent;

    [Header("创建工程")]
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
            // 导出文件夹不算
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
            Popup.ShowMessage("请先填写内容！", Color.red);
        }

        var projectName = projectNameInput.text;
        if (string.IsNullOrEmpty(projectName))
        {
            Popup.ShowMessage("请输入曲名/项目名！", Color.red);
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
        // 需要创建所有难度的谱，并加载这个工程的基础设置
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Tale]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Romance]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.History]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Story]);
        FileUtils.SaveDifficulty(EditingData.EditingMapDic[Difficulty.Revival]);
        EditingData.CurrentEditingMap = EditingData.EditingMapDic[Difficulty.Tale];
        // 创建项目设置并保存
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
            Popup.ShowMessage("创建成功！", Color.green);
            Close();
        }
        else
        {
            Popup.ShowMessage("打开音乐失败！", Color.red);
        }
    }

    private void OnOpenProject(MapDesignerSettings.MapDesignerProjectSetting setting)
    {
        // 因为下边load会用到 ProjectPath 、 ProjectOutPath 这俩路径，所以先赋值
        var musicName = setting.MusicName;
        MapDesignerSettings.LoadSettings(setting);
        var path = $"{GlobalSettings.MapDataPath}/{musicName}";
        var outpath = $"{GlobalSettings.MapDataPath}/{musicName}_out";
        MapDesignerSettings.ProjectPath = path;
        MapDesignerSettings.ProjectOutPath = outpath;

        if (File.Exists($"{path}/audio.{MapDesignerSettings.Ext}"))
        {
            // 打开或创建所有难度的谱
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
                Popup.ShowMessage("打开成功！", Color.green);
            }
            else
            {
                Popup.ShowMessage("打开音乐失败！", Color.red);
            }
        }
        else
        {
            Popup.ShowMessage($"设置里对应路径目前是：{GlobalSettings.MapDataPath}  要打开的音频是./{musicName}/audio.{setting.Ext}" +
                              $"\n请检查实际路径与上述路径是否相符，以及要打开的工程文件夹下的ProjectSetting.json内MusicName是否与文件夹名称相同", Color.yellow);
        }

        Close();
    }

    #endregion
}
