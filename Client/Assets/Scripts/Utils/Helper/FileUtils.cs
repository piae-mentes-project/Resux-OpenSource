using System.IO;
using System.Collections;
using System.Collections.Generic;
using Resux.Data;
using UnityEngine;

namespace Resux
{
    public static class FileUtils
    {
        #region const

        static string savedArchivePath = $"{Assets.AssetsFilePathDM.DataPath}/Saved.txt";

        #endregion

        /// <summary>
        /// 读取存档
        /// </summary>
        /// <returns></returns>
        public static SavedArchive LoadSavedArchive()
        {
            if (!File.Exists(savedArchivePath))
            {
                File.Create(savedArchivePath).Close();
                return new SavedArchive();
            }

            var content = File.ReadAllText(savedArchivePath);
            var archive = Utils.ConvertJsonToObject<SavedArchive>(content);
            if (archive == null)
            {
                Logger.Log("存档文件读取为空，新建一个存档");
                archive = new SavedArchive();
            }

            return archive;
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        public static void SaveArchive(SavedArchive savedArchive)
        {
            if (!File.Exists(savedArchivePath))
            {
                File.Create(savedArchivePath).Close();
            }

            var content = Utils.ConvertObjectToJson(savedArchive);
            File.WriteAllText(savedArchivePath, content);
        }
    }
}
