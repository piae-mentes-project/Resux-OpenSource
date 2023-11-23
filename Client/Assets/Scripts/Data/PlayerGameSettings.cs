using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 玩家的游戏内可调控配置数据
    /// </summary>
    public static class PlayerGameSettings
    {
        #region properties

        #region constant Setting Path

        static string GamePreSettingPath => Assets.AssetsFilePathDM.PlayerSettingPath;

        #endregion

        /// <summary>
        /// 配置数据
        /// </summary>
        public static GamePreSetting Setting;

        #endregion

        static PlayerGameSettings()
        {
            // 游戏预设配置读取
            if (File.Exists(GamePreSettingPath))
            {
                var gamePreSetting = File.ReadAllText(GamePreSettingPath);
                Setting = Utils.ConvertJsonToObject<GamePreSetting>(gamePreSetting);
                Logger.Log(Setting.Language);
            }
            else
            {
                Setting = new GamePreSetting();
                Setting.Language = Application.systemLanguage.ToLanguage();
                SaveGamePreSetting();
            }
            var audioConf = AudioSettings.GetConfiguration();
            audioConf.dspBufferSize = Setting.DspBufferSize;
            AudioSettings.Reset(audioConf);
        }

        #region Public Method

        public static void Initialize()
        {
            // IPlatformInterface.GetImplementation().AddBluetoothHeadsetListener(isOn =>
            // {
            //     Logger.Log($"蓝牙耳机是否已连接：{isOn}");
            //     Setting.IsUsingBluetoothHeadset = isOn;
            // });
        }

        /// <summary>
        /// 保存玩家预设配置
        /// </summary>
        public static void SaveGamePreSetting()
        {
            File.WriteAllText(GamePreSettingPath, Utils.ConvertObjectToJson(Setting));
            // 刷新本地化
            Manager.LocalizedManager.RefreshSetting();
        }

        #endregion
    }
}
