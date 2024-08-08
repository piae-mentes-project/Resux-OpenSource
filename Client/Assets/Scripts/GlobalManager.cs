using System;
using System.Collections;
using System.Collections.Generic;
using Resux.UI;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 全局管理脚本，可能会用到？
    /// </summary>
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance => instance;

        private static GlobalManager instance;

        void Awake()
        {
            instance = this;

            PlayerGameSettings.Initialize();
        }

        private void OnApplicationQuit()
        {
            Data.PlayerRecordManager.SaveRecord();
        }
    }
}
