using System.IO;
using System.Collections;
using System.Collections.Generic;
using Resux.Data;

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
            return Utils.ConvertJsonToObject<SavedArchive>(content);
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
