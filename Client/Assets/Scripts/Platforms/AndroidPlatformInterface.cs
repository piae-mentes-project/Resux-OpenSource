using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    public class AndroidPlatformInterface : IPlatformInterface
    {
        private AndroidJavaObject activityInstance;
        private AndroidJavaObject bluetoothHeadsetReceiver;
        public AndroidPlatformInterface()
        {
            activityInstance = new AndroidJavaClass("com.tenMinStudio.Resux.AndroidPlatform.MainActivity")
                .GetStatic<AndroidJavaObject>("instance");
            bluetoothHeadsetReceiver = activityInstance.Call<AndroidJavaObject>("getBluetoothHeadsetReceiver");
        }

        public void AddBluetoothHeadsetListener(Action<bool> listenerAction)
        {
            var listenerRef = new BluetoothHeadsetListener(listenerAction);
            bluetoothHeadsetReceiver.Call("addListener", listenerRef);
        }

        #region nested class
        private class BluetoothHeadsetListener : AndroidJavaProxy
        {
            private Action<bool> eventListener;
            public BluetoothHeadsetListener(Action<bool> listener)
                : base("com.tenMinStudio.Resux.AndroidPlatform.BluetoothHeadsetCallback")
            {
                this.eventListener = listener;
            }
            public void stateChanged(bool connected)
            {
                eventListener?.Invoke(connected);
            }
        }
        #endregion

        
    }
}
