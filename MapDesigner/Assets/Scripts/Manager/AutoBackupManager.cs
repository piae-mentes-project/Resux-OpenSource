using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AutoBackupManager : MonoBehaviour
{
    public static AutoBackupManager Instance;

    #region properties

    private float time;
    private Queue<string> backupPaths;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        time = 0;
        backupPaths = new Queue<string>();
    }

    void Update()
    {
        if (EditingData.CanEditing)
        {
            time += Time.deltaTime;
            var autoSaveSecond = GlobalSettings.AutoSaveInterval * 60;
            if (time > autoSaveSecond)
            {
                time %= autoSaveSecond;
                _ = Task.Run(AutoSave);
                // 更新当前存档
                UpPanelView.SaveProject();
            }
        }
    }

    #region Public Method

    /// <summary>
    /// 检查现有备份
    /// </summary>
    public void CheckBackups()
    {
        backupPaths.Clear();
        var directories = Directory.GetDirectories(MapDesignerSettings.ProjectPath)
            .Where(path => path.Contains("backup_"))
            .OrderBy(path => path);
        foreach (var directoryPath in directories)
        {
            var directoryName = Path.GetFileName(directoryPath);
            backupPaths.Enqueue($"{MapDesignerSettings.ProjectPath}/{directoryName}");
        }
    }

    #endregion

    #region Private Method

    private async void AutoSave()
    {
        var now = System.DateTime.Now.ToString("yy-MM-dd-HH-mm-ss");
        var maxCount = GlobalSettings.AutoSaveCount;
        string path;
        // 循环删除最旧的备份，直到数量小于最大值
        while (backupPaths.Count >= maxCount)
        {
            path = backupPaths.Dequeue();
            Directory.Delete(path, true);
        }
        // 保存最新的备份
        path = $"{MapDesignerSettings.ProjectPath}/backup_{now}";
        Directory.CreateDirectory(path);
        backupPaths.Enqueue(path);

        await Task.Run(async () =>
        {
            foreach (Difficulty difficulty in System.Enum.GetValues(typeof(Difficulty)))
            {
                using (FileStream fileStream = File.Open($"{MapDesignerSettings.ProjectPath}/{difficulty}.json", FileMode.Open))
                {
                    using (FileStream destinationStream = File.Create($"{path}/{difficulty}.json"))
                    {
                        await fileStream.CopyToAsync(destinationStream);
                    }
                }
            }
        });
    }

    #endregion
}
