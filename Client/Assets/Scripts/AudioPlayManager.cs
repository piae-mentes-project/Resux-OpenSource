using System;
using System.Collections;
using System.Collections.Generic;
using E7.Native;
using Resux.Assets;
using UnityEngine;

namespace Resux.Manager
{
    /// <summary>
    /// 音频管理
    /// </summary>
    public abstract class AudioPlayManager
    {
        private static AudioPlayManager instance;
        public static AudioPlayManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Logger.LogWarning("AudioPlayManager is being calling Instance before init.");
                    Init();
                }
                return instance;
            }
        }

        protected float BGMVolume { get; private set; }
        protected float UIEffectVolume { get; private set; }
        protected float NoteEffectVolume { get; private set; }
        protected bool isPlayingBgm { get; private set; }

        /// <summary>
        /// 请确保在unity主线程中执行，因为非Android/IOS平台用的是UnityAudioPlayManager(
        /// 在未调用Unload的情况下重复调用不会有任何效果
        /// </summary>
        public static void Init()
        {
            if (instance != null) { return; }
            instance =
            #if UNITY_ANDROID && !UNITY_EDITOR
                new NativeAudioPlayManager();
            #else
                new UnityAudioPlayManager(5);
            #endif
            instance.RefreshVolumeSettings();
        }

        /// <summary>
        /// 用设置的音量设置刷新AudioPlayManager
        /// </summary>
        public virtual void RefreshVolumeSettings()
        {
            BGMVolume = PlayerGameSettings.Setting.MainAudioVolume * PlayerGameSettings.Setting.MusicVolume;
            UIEffectVolume = PlayerGameSettings.Setting.MainAudioVolume * PlayerGameSettings.Setting.EffectVolume / 100f;
            NoteEffectVolume = PlayerGameSettings.Setting.MainAudioVolume * PlayerGameSettings.Setting.EffectLoudness / 100f;
        }

        // Effect Part
        /// <summary>
        /// 播放UI音效
        /// TODO: 返回一个int表示当前播放音效的管道id，StopEffect传入管道id停止播放
        /// </summary>
        /// <param name="name">音效名</param>
        /// <param name="volume">音量，小于0时用默认音量</param>
        public abstract void PlayUIEffect(string name, float volume = -1);

        /// <summary>
        /// 播放Note音效
        /// TODO: 返回一个int表示当前播放音效的管道id，StopEffect传入管道id停止播放
        /// </summary>
        /// <param name="name">音效名</param>
        /// <param name="volume">音量，小于0时用默认音量</param>
        public abstract void PlayNoteEffect(string name, float volume = -1);

        // BGM Part
        /// <summary>
        /// BGM正在勃吗？
        /// </summary>
        public abstract bool IsBGMPlaying { get; }
        /// <summary>
        /// BGM，勃起！
        /// </summary>
        /// <param name="name">BGM的名字</param>
        /// <param name="loop">是否循环播放</param>
        public abstract void PlayBGM(string name, bool loop = true);
        /// <summary>
        /// BGM过渡更换
        /// </summary>
        /// <param name="name">bgm名</param>
        /// <param name="fadeTime">过渡时间</param>
        /// <param name="startTime">开始播放的时间（位置）</param>
        /// <param name="loop">是否循环播放</param>
        public abstract void PlayBGM(string name, float fadeTime, float startTime, bool loop = true);
        /// <summary>
        /// BGM你不要再勃辣！
        /// </summary>
        public abstract void StopBGM();

        /// <summary>
        /// 重新播放BGM
        /// </summary>
        public abstract void ReplayBGM(float time = -1);

        /// <summary>
        /// 获取BGM当前进度
        /// </summary>
        /// <returns></returns>
        public abstract float GetBGMTime();

        /// <summary>
        /// 获取当前bgm名称
        /// </summary>
        /// <returns>bgm名称</returns>
        public abstract string GetBGMName();

        public abstract void SetBGMVolume(float volume);

        /// <summary>
        /// 不要了！卸载！释放资源！
        /// </summary>
        public virtual void Unload()
        {
            instance = null;
        }

        #region AudioPlayManager Implements
        /// <summary>
        /// 是TNND的辣鸡unity的音频系统啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊
        /// </summary>
        private class UnityAudioPlayManager : AudioPlayManager
        {
            private readonly AudioSource bgmAudioSource;
            private readonly AudioSource[] effectAudioSources;
            private readonly bool[] effectAudioIsNoteEffect;

            private int effectIndex;

            public bool EffectIsPlaying => effectAudioSources[effectIndex].isPlaying;
            public override bool IsBGMPlaying => bgmAudioSource.isPlaying;

            internal UnityAudioPlayManager(int effectAudioSize)
            {
                var gameObject = new GameObject("[Unity Audio Play Manager]");
                GameObject.DontDestroyOnLoad(gameObject);
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
                bgmAudioSource.playOnAwake = false;

                effectIndex = 0;

                effectAudioSources = new AudioSource[effectAudioSize];
                effectAudioIsNoteEffect = new bool[effectAudioSize];
                for (int i = 0; i < effectAudioSize; i++)
                {
                    effectAudioSources[i] = gameObject.AddComponent<AudioSource>();
                    effectAudioSources[i].loop = false;
                    effectAudioSources[i].playOnAwake = false;
                }
            }

            public override void PlayBGM(string name, bool loop = true)
            {
                if (bgmAudioSource.clip)
                {
                    var currentBgmName = bgmAudioSource.clip.name;
                    // 没在播的话就不管是否同名了
                    if (isPlayingBgm && currentBgmName.Equals(name))
                    {
                        return;
                    }
                }

                StopBGM();
                bgmAudioSource.clip = AudioLoader.BgmLoader.GetResource(name);
                bgmAudioSource.loop = loop;
                bgmAudioSource.Play();
                isPlayingBgm = true;
            }

            public override void PlayBGM(string name, float fadeTime, float startTime, bool loop = true)
            {
                var bgm = AudioLoader.BgmLoader.GetResource(name);
                bgmAudioSource.loop = loop;
                var volume = bgmAudioSource.volume;

                if (bgmAudioSource.clip)
                {
                    var currentBgmName = bgmAudioSource.clip.name;
                    // 没在播的话就不管是否同名了
                    if (isPlayingBgm)
                    {
                        if (currentBgmName.Equals(name))
                        {
                            return;
                        }
                        else
                        {
                            CoroutineUtils.RunPerDelayTime(count =>
                            {
                                if (count < 5)
                                {
                                    bgmAudioSource.volume = volume * (4 - count) / 5;
                                    return;
                                }
                                else if (count == 5)
                                {
                                    StopBGM();
                                    bgmAudioSource.clip = bgm;
                                    bgmAudioSource.time = startTime;
                                    bgmAudioSource.Play();
                                    isPlayingBgm = true;
                                }

                                bgmAudioSource.volume = volume * (count - 4) / 5;
                            }, fadeTime / 10, 10);

                            return;
                        }
                    }
                }

                StopBGM();
                bgmAudioSource.clip = bgm;
                bgmAudioSource.time = startTime;
                bgmAudioSource.Play();
                isPlayingBgm = true;

                CoroutineUtils.RunPerDelayTime(count =>
                {
                    bgmAudioSource.volume = volume * (count + 1) / 5;
                }, fadeTime / 10, 5);
            }

            public override void PlayUIEffect(string name, float volume = -1)
            {
                PlayEffect(name);
                effectAudioSources[effectIndex].volume = volume < 0 ? UIEffectVolume : volume;
                effectAudioIsNoteEffect[effectIndex] = false;
            }

            public override void PlayNoteEffect(string name, float volume = -1)
            {
                PlayEffect(name);
                effectAudioSources[effectIndex].volume = volume < 0 ? NoteEffectVolume : volume;
                effectAudioIsNoteEffect[effectIndex] = true;
            }

            private void PlayEffect(string name)
            {
                effectIndex++;
                effectIndex %= effectAudioSources.Length;
                var effectAudioSource = effectAudioSources[effectIndex];
                effectAudioSource.Stop();
                effectAudioSource.clip = AudioLoader.EffectLoader.GetResource(name);
                effectAudioSource.Play();
            }

            public override void RefreshVolumeSettings()
            {
                base.RefreshVolumeSettings();
                bgmAudioSource.volume = BGMVolume;
                var effectVolume = PlayerGameSettings.Setting.MainAudioVolume * PlayerGameSettings.Setting.EffectVolume / 100f;
                for (int i = 0; i < effectAudioSources.Length; i++)
                {
                    effectAudioSources[i].volume = effectAudioIsNoteEffect[i] ? NoteEffectVolume : UIEffectVolume;
                }
            }

            public override void StopBGM()
            {
                bgmAudioSource.Stop();
                isPlayingBgm = false;
            }

            public override void Unload()
            {
                base.Unload();
                GameObject.Destroy(bgmAudioSource.gameObject);
            }

            public override void SetBGMVolume(float volume)
            {
                bgmAudioSource.volume = volume;
            }

            public override void ReplayBGM(float time = -1)
            {
                if (time < 0)
                {
                    bgmAudioSource.Play();
                }
                else
                {
                    bgmAudioSource.Play();
                    bgmAudioSource.time = time;
                }
            }

            public override float GetBGMTime()
            {
                return bgmAudioSource.time;
            }

            public override string GetBGMName()
            {
                return bgmAudioSource.clip.name;
            }
        }

        /// <summary>
        /// 好耶！是NativeAudio，虽然BGM还是用辣鸡unity的AudioSource
        /// </summary>
        private class NativeAudioPlayManager : AudioPlayManager
        {
            private AudioSource bgmAudioSource;
            private readonly Dictionary<string, NativeAudioPointer> loadedEffectAudio;
            private int currentNativeSourceIndex;
            private readonly bool[] effectAudioIsNoteEffect;
            private NativeSource.PlayOptions effectPlayOptions;

            public override bool IsBGMPlaying => bgmAudioSource.isPlaying;

            internal NativeAudioPlayManager()
            {
                if (!NativeAudio.OnSupportedPlatform) { throw new PlatformNotSupportedException($"{GetType()} is not support on current platform"); }

                var gameObject = new GameObject("[Native Audio Play Manager]");
                GameObject.DontDestroyOnLoad(gameObject);
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
                bgmAudioSource.loop = true;
                bgmAudioSource.playOnAwake = false;

                NativeAudio.Initialize();

                loadedEffectAudio = new Dictionary<string, NativeAudioPointer>();
                currentNativeSourceIndex = 0;
                effectAudioIsNoteEffect = new bool[NativeAudio.GetNativeSourceCount()];
                effectPlayOptions = new NativeSource.PlayOptions();
            }

            public override void PlayBGM(string name, bool loop = true)
            {
                if (bgmAudioSource.clip)
                {
                    var currentBgmName = bgmAudioSource.clip.name;
                    // 没在播的话就不管是否同名了
                    if (isPlayingBgm && currentBgmName.Equals(name))
                    {
                        return;
                    }
                }

                StopBGM();
                bgmAudioSource.clip = AudioLoader.BgmLoader.GetResource(name);
                bgmAudioSource.loop = loop;
                bgmAudioSource.Play();
                isPlayingBgm = true;
            }

            public override void PlayBGM(string name, float fadeTime, float startTime, bool loop = true)
            {
                var bgm = AudioLoader.BgmLoader.GetResource(name);
                bgmAudioSource.loop = loop;
                var volume = bgmAudioSource.volume;

                if (bgmAudioSource.clip)
                {
                    var currentBgmName = bgmAudioSource.clip.name;
                    // 没在播的话就不管是否同名了
                    if (isPlayingBgm)
                    {
                        if (currentBgmName.Equals(name))
                        {
                            return;
                        }
                        else
                        {
                            CoroutineUtils.RunPerDelayTime(count =>
                            {
                                if (count < 5)
                                {
                                    bgmAudioSource.volume = volume * (4 - count) / 5;
                                    return;
                                }
                                else if (count == 5)
                                {
                                    StopBGM();
                                    bgmAudioSource.clip = bgm;
                                    bgmAudioSource.time = startTime;
                                    bgmAudioSource.Play();
                                    isPlayingBgm = true;
                                }

                                bgmAudioSource.volume = volume * (count - 4) / 5;
                            }, fadeTime / 10, 10);

                            return;
                        }
                    }
                }

                StopBGM();
                bgmAudioSource.clip = bgm;
                bgmAudioSource.time = startTime;
                bgmAudioSource.Play();
                isPlayingBgm = true;

                CoroutineUtils.RunPerDelayTime(count =>
                {
                    bgmAudioSource.volume = volume * (count + 1) / 5;
                }, fadeTime / 10, 5);
            }

            public override void PlayUIEffect(string name, float volume = -1)
            {
                effectPlayOptions.volume = volume < 0 ? UIEffectVolume : volume;
                PlayEffect(name);
                effectAudioIsNoteEffect[currentNativeSourceIndex] = false;
            }

            public override void PlayNoteEffect(string name, float volume = -1)
            {
                effectPlayOptions.volume = volume < 0 ? NoteEffectVolume : volume;
                PlayEffect(name);
                effectAudioIsNoteEffect[currentNativeSourceIndex] = true;
            }

            private void PlayEffect(string name)
            {
                // 音量为0就不播了
                if (effectPlayOptions.volume < 1e-5)
                {
                    return;
                }
                NativeAudioPointer pointer;
                if (loadedEffectAudio.ContainsKey(name)) { pointer = loadedEffectAudio[name]; }
                else { pointer = NativeAudio.Load(AudioLoader.EffectLoader.GetResource(name)); }
                var source = NativeAudio.GetNativeSourceAuto();
                currentNativeSourceIndex = source.Index;
                // 不知道为null会怎样
                source.Play(pointer, effectPlayOptions);
            }

            public override void RefreshVolumeSettings()
            {
                base.RefreshVolumeSettings();
                bgmAudioSource.volume = BGMVolume;
                bgmAudioSource.playOnAwake = false;
                var sourceCount = NativeAudio.GetNativeSourceCount();
                for (var i = 0; i < sourceCount; i++)
                {
                    var volume = effectAudioIsNoteEffect[i] ? NoteEffectVolume : UIEffectVolume;
                    NativeAudio.GetNativeSource(i).SetVolume(volume);
                    // 为了防止在播放中更改音量到0，这里也加一句stop
                    if (volume < 1e-5)
                    {
                        NativeAudio.GetNativeSource(i).Stop();
                    }
                }
            }

            public override void StopBGM()
            {
                bgmAudioSource.Stop();
                isPlayingBgm = false;
            }

            public override void Unload()
            {
                base.Unload();
                GameObject.Destroy(bgmAudioSource.gameObject);
                NativeAudio.Dispose();
                foreach (var kvp in loadedEffectAudio)
                {
                    kvp.Value.Unload();
                }
                loadedEffectAudio.Clear();
            }

            public override void SetBGMVolume(float volume)
            {
                bgmAudioSource.volume = volume;
            }

            public override void ReplayBGM(float time = -1)
            {
                if (time < 0)
                {
                    bgmAudioSource.Play();
                }
                else
                {
                    bgmAudioSource.time = time;
                    bgmAudioSource.Play();
                }
            }

            public override float GetBGMTime()
            {
                return bgmAudioSource.time;
            }

            public override string GetBGMName()
            {
                return bgmAudioSource.clip.name;
            }
        }
        #endregion
    }
}
