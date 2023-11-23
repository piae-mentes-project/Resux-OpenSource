using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 反作弊用计时器
    /// </summary>
    public class SystemTimeCounter : MonoBehaviour
    {
        #region properties

        private int lastTime;
        private float lastGameTime;
        private int totalTime;
        private int totalMusicLength;

        /// <summary>因时间流速异常（导致一首曲目的时间内帧数过高）而作弊</summary>
        public bool IsBanned => IsBannedByTimeSpeedException();

        #endregion

        private static SystemTimeCounter instance;

        public static SystemTimeCounter Instance => instance;

        private void Awake()
        {
            // 维持单例
            if (!instance)
            {
                instance = this;
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
                return;
            }
        }

        void Start()
        {
            lastTime = 0;
            lastGameTime = 0;
            totalTime = 0;
            totalMusicLength = 0;
        }

        #region Public Method

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        public void StartCount(int currentTime = 0)
        {
            lastTime = currentTime;
            lastGameTime = Time.time;
            Logger.Log($"Enable - current system time: {DateTime.Now.Millisecond}");
        }

        /// <summary>
        /// 暂停计时
        /// </summary>
        public void Pause()
        {
            var currentTime = GamePlay.MusicPlayer.Instance.GetTrackTime();
            totalMusicLength += currentTime - lastTime;
            totalTime += (int)((Time.time - lastGameTime) * 1000 + 0.5f);
            Logger.Log($"Disable - current system time: {DateTime.Now.Millisecond}");
        }

        #endregion

        #region Private Method

        private bool IsBannedByTimeSpeedException()
        {
            Logger.Log($"总音频时长: {totalMusicLength}, 总游戏时长: {totalTime}", Color.yellow);
#if UNITY_EDITOR
            return false;
#else
            // 误差在1s以内认为没问题
            // return Mathf.Abs(totalTime - totalMusicLength) < 1000;
            return false;
#endif
        }

        #endregion
    }
}