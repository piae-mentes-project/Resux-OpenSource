using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 平台接口的C#封装
    /// </summary>
    public interface IPlatformInterface
    {
        #region Interface Methods

        /// <summary>
        /// 添加蓝牙插入事件监听
        /// </summary>
        void AddBluetoothHeadsetListener(Action<bool> listenerAction);

        #endregion

        #region Static Methods

        private static IPlatformInterface interfaceImplementation = null;

        private static void CreateImplementation()
        {
#if UNITY_ANDROID
            interfaceImplementation = new AndroidPlatformInterface();
#elif UNITY_IOS
            interfaceImplementation = new IOSPlatformInterface();
#endif
        }

        public static IPlatformInterface GetImplementation()
        {
            if(interfaceImplementation == null) CreateImplementation();
            return interfaceImplementation;
        }

        #endregion
    }
}
