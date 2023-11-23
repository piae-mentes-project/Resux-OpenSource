using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 文件操作类
/// </summary>
public static class FileUtils
{
    #region System dll

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] FileOpenDialog ofd);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName([In, Out] FileOpenDialog ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

    #endregion

    #region Public Method

    public static string GetMusicFilePath(string[] exts, out string selectExt, out string fileName)
    {
        if (exts == null || exts.Length == 0)
        {
            fileName = selectExt = "";
            return "";
        }
        FileOpenDialog dialog = new FileOpenDialog();
        dialog.structSize = Marshal.SizeOf(dialog);
        string extStr = $"*.{exts[0]}";
        if (exts.Length > 1)
        {
            for (int i = 1; i < exts.Length; i++)
            {
                extStr += $";*.{exts[i]}";
            }
        }
        dialog.filter = $"音乐文件({extStr})\0{extStr}";
        dialog.file = new string(new char[256]);
        dialog.maxFile = dialog.file.Length;
        dialog.fileTitle = new string(new char[64]);
        dialog.maxFileTitle = dialog.fileTitle.Length;
        dialog.initialDir = "s";
        dialog.title = "选取音乐";
        dialog.defExt = exts[0];
        dialog.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (GetOpenFileName(dialog))
        {
            var filePath = dialog.file;
            fileName = dialog.fileTitle;
            selectExt = filePath.Substring(dialog.fileExtension);
            return filePath;
        }
        else
        {
            fileName = selectExt = "";
            return "";
        }
    }

    public static string GetFolderPath(string title = "选择路径")
    {
        OpenDialogDir ofn2 = new OpenDialogDir();
        ofn2.pszDisplayName = new string(new char[2048]);
        // 存放目录路径缓冲区  
        ofn2.lpszTitle = title; // 标题  
        ofn2.ulFlags = 0x00000040; // 新的样式,带编辑框  
        IntPtr pidlPtr = SHBrowseForFolder(ofn2);

        char[] charArray = new char[2048];

        for (int i = 0; i < 2048; i++)
        {
            charArray[i] = '\0';
        }

        SHGetPathFromIDList(pidlPtr, charArray);
        string res = new string(charArray);
        res = res.Substring(0, res.IndexOf('\0'));
        return res;

    }

    public static Task SaveDifficulty(InGameMusicMap mapData)
    {
        return File.WriteAllTextAsync(GetMapPath(mapData.difficulty), JsonConvert.SerializeObject(mapData));
    }

    public static string GetMapPath(Difficulty difficulty, bool isOut = false)
    {
        return isOut ? $"{MapDesignerSettings.ProjectOutPath}/{difficulty}.json" : $"{MapDesignerSettings.ProjectPath}/{difficulty}.json";
    }

    public static InGameMusicMap LoadDifficulty(Difficulty difficulty)
    {
        var path = GetMapPath(difficulty);
        if (!File.Exists(path))
        {
            SaveDifficulty(EditingData.EditingMapDic[difficulty]);
            return EditingData.EditingMapDic[difficulty];
        }
        else
        {
            return LoadEditMusicMap(path, difficulty);
        }
    }

    public static InGameMusicMap LoadEditMusicMap(string path, Difficulty difficulty)
    {
        StreamReader reader = File.OpenText(path);
        string tData = reader.ReadToEnd();
        reader.Close();
        InGameMusicMap map = Utils.ConvertJsonToObject<InGameMusicMap>(tData);

        // 处理存储难度不对的情况
        if (map != null && map.difficulty != difficulty)
        {
            map.difficulty = difficulty;
        }
        // 处理没有自定义曲线的情况
        foreach (var judgeNoteInfo in map.judgeNotes)
        {
            if (judgeNoteInfo.HoldCurves == null || judgeNoteInfo.HoldCurves.Count == 0)
            {
                judgeNoteInfo.InitHoldCurves();
            }
        }

        return map;
    }

    /// <summary>
    /// 将数据转化为json并保存
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void SaveJsonFile(string path, object data)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(data));
    }

    /// <summary>
    /// 从文件读取Json数据
    /// </summary>
    /// <typeparam name="T">结构类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns>Json数据</returns>
    public static T GetDataFromJsonFile<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            StreamReader reader = File.OpenText(path);
            string tData = reader.ReadToEnd();
            reader.Close();
            var dataRes = JsonConvert.DeserializeObject<T>(tData);
            return dataRes;
        }
        catch (Exception e)
        {
            Debug.LogError($"exception file path: {path}\n{e.Message}:\n {e.StackTrace}");
            return null;
        }
    }

    // source from https://stackoverflow.com/questions/49858310/how-to-async-md5-calculate-c-sharp
    public static async Task<string> CalculateFileMD5(string filename)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                do
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, 8192);
                    if (bytesRead > 0)
                    {
                        md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }
                } while (bytesRead > 0);

                md5.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToUpperInvariant();
            }
        }
    }

    #endregion
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class FileOpenDialog
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public string filter = null;
    public string customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public string file = null;
    public int maxFile = 0;
    public string fileTitle = null;
    public int maxFileTitle = 0;
    public string initialDir = null;
    public string title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public string defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public string templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
    public FileOpenDialog()
    {
        dlgOwner = GetForegroundWindow();
    }
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenDialogDir
{
    public IntPtr hwndOwner = IntPtr.Zero;
    public IntPtr pidlRoot = IntPtr.Zero;
    public String pszDisplayName = null;
    public String lpszTitle = null;
    public UInt32 ulFlags = 0;
    public IntPtr lpfn = IntPtr.Zero;
    public IntPtr lParam = IntPtr.Zero;
    public int iImage = 0;
}
