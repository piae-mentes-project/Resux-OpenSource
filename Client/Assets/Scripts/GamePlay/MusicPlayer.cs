using System;
using System.Collections;
using System.Diagnostics;
using Resux.Assets;
using UnityEngine;
using System.Threading.Tasks;
using Resux.LevelData;
using Resux.Manager;
using UnityEngine.Networking;

namespace Resux.GamePlay
{
    /// <summary>
    /// 音乐播放器
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        #region properties

        public static MusicPlayer Instance;

        /// <summary>
        /// 音源
        /// </summary>
        [SerializeField]
        private AudioSource audioSource;

        /// <summary>
        /// 音乐
        /// </summary>
        private AudioClip music;
        /// <summary>音乐长度</summary>
        private float musicLength;
        /// <summary>播放完毕</summary>
        private bool isOver;
        private Action OnMusicPlayEnd;

        private double trackStartTime;
        private double pauseTime;
        private double currentMusicTime;
        private int frameCount;
        private double lastUpdateDspTime;
        private double interpolationDspTime;
        private double lastDspTIme;

        #endregion

        #region UnityEngine

        void Start()
        {
            Instance = this;
            // load settings
            AudioPlayManager.Instance.RefreshVolumeSettings();
            audioSource.volume = PlayerGameSettings.Setting.MainAudioVolume * PlayerGameSettings.Setting.MusicVolume;
            lastUpdateDspTime = interpolationDspTime = AudioSettings.dspTime;
        }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Update()
        {
            var currentDspTime = AudioSettings.dspTime;
            if (lastUpdateDspTime == currentDspTime)
            {
                interpolationDspTime += Time.deltaTime;
            }
            else
            {
                lastUpdateDspTime = interpolationDspTime = currentDspTime;
            }
            if (isOver)
            {
                return;
            }
            // 时间计算
            if (audioSource.isPlaying)
            {

                #region Old Code 老代码
                // var targetTime = AudioSettings.dspTime - trackStartTime;
                // frameCount++;
                // if (frameCount >= 60)
                // {
                //     frameCount = 0;
                //     deltaDspTime = AudioSettings.dspTime - lastDspTIme;
                //     lastDspTIme = AudioSettings.dspTime;
                // }
                //
                // if (deltaDspTime <= 0)
                // {
                //     if (targetTime > currentMusicTime)
                //     {
                //         currentMusicTime = targetTime;
                //     }
                // }
                // else
                // {
                //     var avgTime = deltaDspTime / 60;
                //     if (currentMusicTime + avgTime < targetTime)
                //     {
                //         currentMusicTime += avgTime;
                //     }
                //     else
                //     {
                //         currentMusicTime = targetTime;
                //     }
                // }
                #endregion
                currentMusicTime = interpolationDspTime - trackStartTime;
                // SetMusicSample((int)(currentMusicTime * audioSource.clip.frequency + 0.5f));
            }

            if (audioSource.clip && !audioSource.isPlaying && audioSource.time == 0f)
            {
                // 在上边更新时间会导致最后差一帧的时间

                currentMusicTime = interpolationDspTime - trackStartTime;
                if (Mathf.Abs((float)(currentMusicTime - musicLength)) >= 1e-7)
                {
                    isOver = true;
                    OnMusicPlayEnd?.Invoke();
                }
            }
        }

        #endregion

        #region Private Method

        private void SetMusicSample(int timeSample)
        {
            audioSource.timeSamples = timeSample;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// 初始化调用
        /// </summary>
        public void Initialize(LevelData.LevelDetail config)
        {
            SetMusic(config);
        }

        public void AddMusicEndListener(Action action)
        {
            OnMusicPlayEnd += action;
        }

        /// <summary>
        /// 设置音乐
        /// </summary>
        public void SetMusic(LevelData.LevelDetail config)
        {
            music = AudioLoader.GetMusic(config._songName);
            musicLength = music.length;
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        public void PlayMusic()
        {
            audioSource.clip = music;
            lastDspTIme = trackStartTime = interpolationDspTime;
            audioSource.Play();
        }

        /// <summary>
        /// 暂停音乐的播放
        /// </summary>
        public void PauseMusic()
        {
            pauseTime = interpolationDspTime;
            audioSource.Pause();
        }

        public int GetTrackTime()
        {
            // from https://www.jianshu.com/p/0a065da7e106
            return (int)(currentMusicTime * 1000 + 0.5f);
        }

        /// <summary>
        /// 继续音乐的播放
        /// </summary>
        /// <param name="backSeconds">回溯的时间（s）</param>
        public void ContinueMusic(float backSeconds = 0)
        {
            // 当前播放时间（假设不暂停）=原播放时间+暂停时间+后退时间
            trackStartTime += interpolationDspTime - pauseTime + backSeconds;
            var musicTime = audioSource.time;
            if (backSeconds >= musicTime)
            {
                audioSource.timeSamples = 0;
            }
            else
            {
                // from https://www.jianshu.com/p/0a065da7e106
                SetMusicSample((int)((musicTime - backSeconds) * audioSource.clip.frequency + 0.5f));
            }
            audioSource.UnPause();
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        /// <summary>
        /// 获取音乐的时长
        /// </summary>
        /// <returns>音乐时长</returns>
        public float GetMusicLength()
        {
            return music == null ? 0 : musicLength;
        }

        /// <summary>
        /// 播放特效音
        /// </summary>
        /// <param name="result">特效类型</param>
        public void PlayEffect(JudgeType judgeType)
        {
            AudioPlayManager.Instance.PlayNoteEffect(judgeType.ToString());
        }

        #endregion
    }
}
