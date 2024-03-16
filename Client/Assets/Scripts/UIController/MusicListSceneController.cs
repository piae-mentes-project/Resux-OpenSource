using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resux.GamePlay;
using Resux.UI;
using Resux.Assets;
using Resux.Configuration;
using Resux.Data;
using Resux.LevelData;
using Resux.Manager;

namespace Resux.UI.Manager
{
    /// <summary>
    /// 主界面UI管理
    /// </summary>
    public class MusicListSceneController : MonoBehaviour
    {
        public static class Data
        {
            public static Texture2D backgroundTex;
        }

        private class CoverTextureProvider : ScrollImage.ITextureProvider, IDisposable
        {
            private int _loadAhead;
            private ScrollImage _scrollImage;
            private LevelData.LevelDetail[] _musicConfigs;

            /// <param name="loadAhead">预加载背景图个数</param>
            public CoverTextureProvider(ScrollImage scrollImage, int loadAhead, LevelData.LevelDetail[] musicConfigs)
            {
                _scrollImage = scrollImage;
                _loadAhead = loadAhead;
                _musicConfigs = musicConfigs;
            }

            public Texture2D GetTexture(int index)
            {
                if (index < 0 || index >= _musicConfigs.Length) { return null; }
                LoadTexture(index);
                for (int i = 1; i <= _loadAhead; i++)
                {
                    LoadTexture(index + i);
                    LoadTexture(index - i);
                }
                // 在方法内已经追加_Cover了
                return ImageLoader.GetMusicCover(_musicConfigs[index]._songName);
            }

            private void LoadTexture(int index)
            {
                if (index < 0 || index >= _musicConfigs.Length) { return; }
                var name = _musicConfigs[index]._songName;
                _scrollImage.ReplaceTexture(ImageLoader.GetMusicCover(name), index);
            }

            public void Dispose()
            {

            }
        }

        public static GameScene ThisScene => GameScene.MusicListScene;

        #region properties

        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Text titleText;
        [SerializeField] private RawImage backgroundImage;

        [SerializeField] private AudioSource audioSource; // 预览音乐
        [SerializeField] private AnimationCurve musicVolumeCurve;

        [SerializeField] private ExtendToggleGroup difficultyToggleGroup;
        [SerializeField] private ScrollImage musicScroll;
        [SerializeField] private Button backButton;
        [SerializeField] private Text backBtnLabelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Text playBtnLabelText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;

        [SerializeField] private Toggle[] difficultyToggles;
        [SerializeField] private Text[] difficultyTexts;
        [SerializeField] private Text[] difficultyLabels;
        [SerializeField] private GameObject[] difficultyDisableLabels;

        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text bestScoreLabel;

        private LevelData.LevelDetail[] _musicConfigs;

        private float _prevStartTime;
        private Coroutine _prevCoroutine;

        #endregion

        #region UnityEngine

        private void Awake()
        {
            // 从上一Scene获取章节选择信息
            var musicGroup = MusicScoreSelection.ChapterDetail;
            Logger.Log($"Selection Group Id: {musicGroup.id}");

            // 暂停BGM，准备播放曲目BGM；载入预览所需资源
            AudioPlayManager.Instance.StopBGM();
            AudioLoader.LoadMusicBundle(musicGroup.id);
            MapLoader.LoadMapBundle(musicGroup.id);
            ImageLoader.LoadMusicCoverBundle(musicGroup.id);

            // 显示3个难度的标题
            for (int i = 0; i < difficultyLabels.Length; i++)
            {
                if (difficultyLabels[i])
                {
                    difficultyLabels[i].text = ((Difficulty)i).GetName();
                }
            }

            // 设置按钮事件；开始模糊背景图片
            backButton.onClick.AddListener(GlobalStaticUIManager.Instance.BackToPrevScene);
            playButton.onClick.AddListener(OnPlayClick);
            // backgroundImage.texture = Data.backgroundTex;
            backgroundImage.GetComponent<UI.Component.Effect.ImageBlurGPU>().StartBlur(false);
            _musicConfigs = musicGroup.musicConfigs.ToArray();
            Logger.Log($"music volume: {PlayerGameSettings.Setting.MusicVolume}", Color.yellow);

            var index = -1;

            // 防止因为初始没有值导致定位到最后一个
            if (_musicConfigs.Any(config => config._id == MusicScoreSelection.LevelDetail._id))
            {
                foreach (var config in _musicConfigs)
                {
                    index++;
                    if (config._id == MusicScoreSelection.LevelDetail._id) { break; }
                }
            }
            else
            {
                index = 0;
            }
            musicScroll.TextureProvider = new CoverTextureProvider(musicScroll, 2, _musicConfigs);
            musicScroll.Init(_musicConfigs.Length - 1, index);
            MusicScoreSelection.LevelDetail = _musicConfigs[index];
            difficultyToggleGroup.Initialize((int)MusicScoreSelection.Difficulty);
            difficultyToggleGroup.onSelectChanged.AddListener(OnDifficultySelectChanged);
            musicScroll.onIndexChanged.AddListener(OnMusicScrollIndexChanged);
            prevButton.onClick.AddListener(OnPrevButtonClick);
            nextButton.onClick.AddListener(OnNextButtonClick);
        }

        private void Start()
        {
            OnMusicScrollIndexChanged();
            StartCoroutine(AsyncLoadAllMusicCover());
            StartCoroutine(AsyncLoadAllMusic());
        }

        // 已经改成根据最后加载时间自动延迟卸载了
        // private void OnDestroy()
        // {
        //     // 卸载ab包，减少资源占用（会延迟卸载，当尚未卸载时再次加载则会中断卸载）
        //     // AudioLoader.UnloadMusicBundle();
        //     // MapLoader.UnloadMapBundle();
        //     // ImageLoader.UnloadMusicCoverBundle();
        // }

        #endregion

        #region Private Method

        private void OnMusicScrollIndexChanged()
        {
            var config = _musicConfigs[musicScroll.Index];
            MusicScoreSelection.LevelDetail = config;

            nextButton.interactable = musicScroll.Index < _musicConfigs.Length - 1;
            prevButton.interactable = musicScroll.Index > 0;

            for (int i = 0; i < difficultyToggles.Length; i++)
            {
                var hasDifficulty = config.ContainsDifficulty(i);
                if (difficultyToggles[i] != null)
                {
                    difficultyToggles[i].interactable = hasDifficulty;
                }
                if (difficultyTexts[i] != null)
                {
                    difficultyTexts[i].text = config.GetLevelStr(i);
                }
                if (difficultyDisableLabels[i] != null)
                {
                    difficultyDisableLabels[i].SetActive(!hasDifficulty);
                }
            }
            Logger.Log($"select difficulty: {MusicScoreSelection.Difficulty}, scroll", Color.yellow);
            for (int i = (int)MusicScoreSelection.Difficulty; i > 0; i--)
            {
                if (config.ContainsDifficulty(i))
                {
                    difficultyToggles[i].isOn = true;
                    MusicScoreSelection.Difficulty = (Difficulty)i;
                    break;
                }
            }

            RefreshMyBest();

            ChangeMusicPreview(); // 更改预览音乐的音频
        }

        private void OnDifficultySelectChanged(Toggle toggle)
        {
            var index = 0;
            foreach (var t in difficultyToggles)
            {
                if (toggle == t) { break; }
                index++;
            }
            MusicScoreSelection.Difficulty = (Difficulty)index;
            Logger.Log($"select difficulty: {MusicScoreSelection.Difficulty}", Color.yellow);

            RefreshMyBest();

            // 播放音效
            AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.ChangeMusic);
        }

        private void RefreshMyBest()
        {
            var record = PlayerRecordManager.QueryPlayResultRecord(MusicScoreSelection.LevelDetail._id, MusicScoreSelection.Difficulty);
            bestScoreText.text = record == null ? "0" : record.Score.ToString();
        }

        private void OnPlayClick()
        {
            // 检查关卡文件是否存在
            // TODO:有空写冷更新...
            if (!MusicScoreSelection.LevelDetail.ContainsDifficulty(MusicScoreSelection.Difficulty))
            {
                return;
            }

            // 禁用UI交互
            canvasGroup.interactable = false;
            musicScroll.SetDragEnable(false);

            StopCoroutine(_prevCoroutine);
            StartCoroutine(EnterPlay());
        }

        private void OnNextButtonClick() => musicScroll.SelectNext();
        private void OnPrevButtonClick() => musicScroll.SelectPrevious();

        private void ChangeMusicPreview()
        {
            if (_prevCoroutine != null) StopCoroutine(_prevCoroutine);

            audioSource.Pause();

            // 选歌音效
            AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.ChangeMusic);

            _prevCoroutine = StartCoroutine(CO_UpdateMusicVolume());
        }

        #endregion

        #region Coroutine

        private IEnumerator AsyncLoadAllMusic()
        {
            var currentIndex = musicScroll.Index;
            var totalMusicCount = _musicConfigs.Length;
            // 把当前索引也算上，所以加载范围至少为1，最长为整个长度
            var maxLength = Mathf.Max(currentIndex + 1, totalMusicCount - currentIndex);
            string musicName;
            for (int i = 0, index = 0; i < maxLength; i++)
            {
                // 先加载前边一个
                index = currentIndex - i;
                if (index >= 0)
                {
                    musicName = _musicConfigs[index]._songName;
                    if (!AudioLoader.IsMusicLoaded(musicName))
                    {
                        var clip = AudioLoader.LoadMusicAsync(musicName);
                        yield return new WaitUntil(() => clip.isDone);
                        AudioLoader.AddMusic(musicName, clip.asset as AudioClip);
                        Logger.Log($"{musicName} loaded!", Color.yellow);
                    }
                }

                // 再加载后边一个
                index = currentIndex + i;
                if (index < totalMusicCount)
                {
                    musicName = _musicConfigs[index]._songName;
                    if (!AudioLoader.IsMusicLoaded(musicName))
                    {
                        var clip = AudioLoader.LoadMusicAsync(musicName);
                        yield return new WaitUntil(() => clip.isDone);
                        AudioLoader.AddMusic(musicName, clip.asset as AudioClip);
                        Logger.Log($"{musicName} loaded!", Color.yellow);
                    }
                }
            }
        }

        private IEnumerator AsyncLoadAllMusicCover()
        {
            var currentIndex = musicScroll.Index;
            var totalMusicCount = _musicConfigs.Length;
            // 把当前索引也算上，所以加载范围至少为1，最长为整个长度
            var maxLength = Mathf.Max(currentIndex + 1, totalMusicCount - currentIndex);
            string musicName;
            for (int i = 0, index = 0; i < maxLength; i++)
            {
                // 先加载前边一个
                index = currentIndex - i;
                if (index >= 0)
                {
                    musicName = _musicConfigs[index]._songName;
                    if (!ImageLoader.IsMusicCoverLoaded(musicName))
                    {
                        var clip = ImageLoader.LoadMusicCoverAsync(_musicConfigs[index]._songName);
                        yield return new WaitUntil(() => clip.isDone);
                        ImageLoader.AddMusicCover(musicName, clip.asset as Texture2D);
                        Logger.Log($"{musicName} Cover loaded!", Color.yellow);
                    }
                }

                // 再加载后边一个
                index = currentIndex + i;
                if (index < totalMusicCount)
                {
                    musicName = _musicConfigs[index]._songName;
                    if (!ImageLoader.IsMusicCoverLoaded(musicName))
                    {
                        var clip = ImageLoader.LoadMusicCoverAsync(_musicConfigs[index]._songName);
                        yield return new WaitUntil(() => clip.isDone);
                        ImageLoader.AddMusicCover(musicName, clip.asset as Texture2D);
                        Logger.Log($"{musicName} Cover loaded!", Color.yellow);
                    }
                }
            }
        }

        private IEnumerator CO_UpdateMusicVolume()
        {
            var index = musicScroll.Index;
            var config = _musicConfigs[index];
            var musicName = _musicConfigs[index]._songName;
            var baseVolume = PlayerGameSettings.Setting.MusicVolume;

            yield return new WaitUntil(() => AudioLoader.IsMusicLoaded(musicName));
            audioSource.clip = AudioLoader.GetMusic(musicName);
            
            const float appearTime = 1f; // 淡入淡出时间
            const float delayTime = 0.5f; // 停止缓冲时间

            var startTime = config.musicPreviewRange.x / 1000f;
            var endTime = config.musicPreviewRange.y / 1000f;

            if (startTime <= 0f)
            {
                Logger.LogWarning("音频开始预览时间有毛病, 已自动调节");
                startTime = audioSource.clip.length / 2f - appearTime - 5f;
            }
            if (endTime - startTime < appearTime * 2f)
            {
                Logger.LogWarning("音频结束预览时间有毛病, 已自动调节");
                endTime = startTime + appearTime * 2f + 15f;
            }

            var length = endTime - startTime;

            yield return new WaitForSecondsRealtime(delayTime);

            _prevStartTime = Time.time;
            audioSource.Pause();
            audioSource.time = startTime;
            audioSource.Play();

            while (true)
            {
                var duration = Time.time - _prevStartTime;

                if (duration <= length)
                {
                    if (duration < appearTime)
                    {
                        audioSource.volume = musicVolumeCurve.Evaluate(duration / appearTime) * baseVolume;
                    }
                    else if (duration > length - appearTime)
                    {
                        audioSource.volume = musicVolumeCurve.Evaluate((length - duration) / appearTime) * baseVolume;
                    }
                    else
                    {
                        audioSource.volume = baseVolume;
                    }
                }
                else
                {
                    _prevStartTime = Time.time;
                    audioSource.Pause();
                    audioSource.time = startTime;
                    audioSource.Play();
                }

                yield return new WaitForEndOfFrame();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator EnterPlay()
        {
            // 设置并播放音效
            AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.StartPlay);

            // 音乐淡出
            var volume = audioSource.volume;
            var cutoutTime = 0.8f;
            var starttime = Time.time;
            var currentTime = 0f;
            var waitForFrameEnd = new WaitForEndOfFrame();

            while (currentTime < cutoutTime)
            {
                currentTime = Time.time - starttime;
                // animation curve没办法通过y得到x，所以只能这样了
                audioSource.volume = musicVolumeCurve.Evaluate(1 - currentTime / cutoutTime) * volume;
                yield return waitForFrameEnd;
            }

            // yield return new WaitForSeconds(AudioPlayManager.Instance.GetCurrentEffectLength());

            Navigator.JumpScene(GameScene.PlayScene);
        }

        #endregion
    }
}