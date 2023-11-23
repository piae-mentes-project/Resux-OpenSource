using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    public class IOSPlatformInterface : IPlatformInterface
    {
        public void AddBluetoothHeadsetListener(Action<bool> listenerAction)
        {
            // TODO: 蓝牙耳机连接的监听
            throw new NotImplementedException();
        }
    }
}
