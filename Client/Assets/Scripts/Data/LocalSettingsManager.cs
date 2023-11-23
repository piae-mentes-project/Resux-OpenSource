using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Resux.Data
{
    /// <summary>
    /// 用于保存/读取用户可编辑的数据
    /// </summary>
    public class LocalSettingsManager
    {
        private static string SettingsPath { get; } = Application.persistentDataPath + "/settings.dat";

        #region Private Methods

        private static void CheckFileExists()
        {
            if (!File.Exists(SettingsPath))
            {
                File.Create(SettingsPath).Close();
            }
        }

        private static Dictionary<string, string> ReadAllSettings()
        {
            CheckFileExists();
            Dictionary<string, string> result = new Dictionary<string, string>();
            var contentRaw = File.ReadAllText(SettingsPath).Split('\n');
            foreach (var line in contentRaw)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var kvPair = line.Split(new char[] { '=' }, 2);
                result[kvPair[0]] = kvPair[1].Replace("\\n", "\n").Replace("\\\\", "\\");
            }
            return result;
        }

        private static void WriteAllSettings(Dictionary<string, string> settings)
        {
            CheckFileExists();
            string result = "";
            foreach (var item in settings)
            {
                result += item.Key + "=";
                result += item.Value.Replace("\n", "\\n").Replace("\\", "\\\\") + "\n";
            }
            File.WriteAllText(SettingsPath, result);
        }

        #endregion

        #region Public Methods

        public static void Write(string key, string value)
        {
            Dictionary<string, string> itemMap = ReadAllSettings();
            itemMap[key] = value;
            WriteAllSettings(itemMap);
        }

        public static string Read(string key, string defaultValue = "", bool writeDefaltValue = false)
        {
            Dictionary<string, string> itemMap = ReadAllSettings();
            string result = "";
            if (!itemMap.ContainsKey(key))
            {
                result = defaultValue;
                if (writeDefaltValue)
                {
                    Write(key, defaultValue);
                }
            }
            else
            {
                result = itemMap[key];
            }
            return result;
        }

        #endregion
    }
}