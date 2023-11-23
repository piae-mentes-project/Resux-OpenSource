using System;
using System.Collections;
using System.Collections.Generic;
using Resux.UI;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// ȫ�ֹ���ű������ܻ��õ���
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
