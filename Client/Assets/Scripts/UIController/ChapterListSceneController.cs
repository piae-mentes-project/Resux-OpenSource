using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Resux.UI;
using Resux.Data;
using Resux.GamePlay;
using Resux.Assets;
using Resux.Configuration;
using Resux.Manager;
using UnityEngine.Rendering;

namespace Resux.UI.Manager
{
    /// <summary>
    /// 曲目选集场景
    /// </summary>
    public class ChapterListSceneController : MonoBehaviour
    {
        public static class Data
        {
            public static Texture2D BackgroundTex;
        }

        private class CoverTextureProvider : ScrollImage.ITextureProvider, IDisposable
        {
            private int loadAhead;
            private ScrollImage scrollImage;

            /// <param name="loadAhead">预加载背景图个数</param>
            public CoverTextureProvider(ScrollImage scrollImage, int loadAhead)
            {
                this.scrollImage = scrollImage;
                this.loadAhead = loadAhead;
            }

            public Texture2D GetTexture(int index)
            {
                if (index < 0 || index >= MusicDataDM.MusicGroups.Count) { return null; }
                LoadTexture(index);
                for (int i = 1; i <= loadAhead; i++)
                {
                    LoadTexture(index + i);
                    LoadTexture(index - i);
                }
                return ImageLoader.GlobalImageLoader.GetResource(MusicDataDM.MusicGroups[index].name);
            }

            private void LoadTexture(int index)
            {
                if (index < 0 || index >= MusicDataDM.MusicGroups.Count) { return; }
                var name = MusicDataDM.MusicGroups[index].name;
                scrollImage.ReplaceTexture(ImageLoader.GlobalImageLoader.GetResource(name), index);
            }

            public void Dispose()
            {

            }
        }

        #region properties

        public static GameScene ThisScene => GameScene.ChapterListScene;

        #region Scene Object

        [SerializeField] private RawImage backgroundImage;
        [Space]
        [SerializeField] private Text sceneTitle;
        [SerializeField] private Button backButton;
        [SerializeField] private Text backBtnLabel;
        [SerializeField] private DraggableUIElement draggableElement;
        [SerializeField] private GameObject nextImage;
        [SerializeField] private GameObject prevImage;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [Space]
        [SerializeField] private Text musicGroupName;
        [SerializeField] private ScrollImage coverScroll;
        [Space]
        [SerializeField] private Text chapterName;
        [SerializeField] private GameObject nextChapterArea;
        [SerializeField] private Text nextChapterTitle;
        [SerializeField] private Text nextChapterName;
        [Space]
        [SerializeField] private Text totalMusicCount;
        [SerializeField] private Text totalMusicTitle;
        [SerializeField] private Text clearCount;
        [SerializeField] private Text clearTitle;
        [SerializeField] private Text fcCount;
        [SerializeField] private Text fcTitle;
        [SerializeField] private Text apCount;
        [SerializeField] private Text apTitle;
        [SerializeField] private Text lockedCount;
        [SerializeField] private Text lockedTitle;
        [Space]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AudioListener audioListener;

        #endregion

        [SerializeField] private int textureLoadAhead = 2;

        private List<ChapterDetail> MusicGroups => MusicDataDM.MusicGroups;

        #endregion

        void Awake()
        {
            coverScroll.TextureProvider = new CoverTextureProvider(coverScroll, textureLoadAhead);
            coverScroll.Init(MusicDataDM.MusicGroups.Count - 1, MusicScoreSelection.MusicGroupIndex);
            coverScroll.onIndexChanged.AddListener(() => { OnMusicGroupChange(coverScroll.Index); });
            draggableElement.onClick += e =>
            {
                var target = e.pointerCurrentRaycast.gameObject;
                var chapterOffset = 0;
                if (target == prevImage) chapterOffset = -1;
                else if (target == nextImage) chapterOffset = 1;
                TurnToMusicSelectScene(chapterOffset);
            };
            nextButton.onClick.AddListener(OnNextButtonClick);
            prevButton.onClick.AddListener(OnPrevButtonClick);
            backButton.onClick.AddListener(GlobalStaticUIManager.Instance.BackToPrevScene);
            // backgroundImage.texture = Data.BackgroundTex;
            backgroundImage.GetComponent<UI.Component.Effect.ImageBlurGPU>().StartBlur(false);
            OnMusicGroupChange(MusicScoreSelection.MusicGroupIndex);

            AudioPlayManager.Instance.PlayBGM(Sounds.Bgm.RootScene);
            StartCoroutine(AsyncLoadAllMusicGroupCover());
        }

        void OnDestroy()
        {

        }

        #region Private Method

        private void TurnToMusicSelectScene(int chapterOffset)
        {
            Logger.Log("Go To MainScene");
            StartCoroutine(OnAddtiveLoad(tex2D =>
            {
                MusicListSceneController.Data.backgroundTex = tex2D;
                MusicScoreSelection.MusicGroupIndex = Math.Clamp(MusicScoreSelection.MusicGroupIndex + chapterOffset,
                    0, MusicDataDM.MusicGroups.Count - 1); // 限制住值的大小防止用户用切换卡片间的动画卡出bug
                Navigator.LoadNewScene(GameScene.MusicListScene);
            }));
        }

        private void OnMusicGroupChange(int index)
        {
            AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.ChangeMusic);

            var groups = MusicDataDM.MusicGroups;
            MusicScoreSelection.MusicGroupIndex = index;
            var musicGroup = groups[index];
            musicGroupName.text = musicGroup.name.Localize();

            nextButton.interactable = index < MusicDataDM.MusicGroups.Count - 1;
            prevButton.interactable = index > 0;

            var isShowNextChapter = index < groups.Count - 1;
            nextChapterArea.SetActive(isShowNextChapter);
            // 下一曲集的名称显示
            if (isShowNextChapter)
            {
                var nextGroup = groups[index + 1];
                nextChapterName.text = nextGroup.name.Localize();
            }

            // 曲集中的数据显示
            var currentMusicList = musicGroup.musicConfigs;
            totalMusicCount.text = currentMusicList.Count.ToString();
            clearCount.text = PlayerRecordManager.QueryRecordCountInList(currentMusicList,
                MusicScoreSelection.Difficulty,
                record => record.Score > 0).ToString();
            fcCount.text = PlayerRecordManager.QueryRecordCountInList(currentMusicList,
                MusicScoreSelection.Difficulty,
                record => record.BestScoreType >= ScoreType.FullCombo).ToString();
            apCount.text = PlayerRecordManager.QueryRecordCountInList(currentMusicList,
                MusicScoreSelection.Difficulty,
                record => record.BestScoreType >= ScoreType.AllPerfect).ToString();
            // TODO: 未解锁数量
            lockedCount.text = "0";
        }

        private void OnNextButtonClick()
        {
            coverScroll.SelectNext();
        }

        private void OnPrevButtonClick()
        {
            coverScroll.SelectPrevious();
        }

        #endregion

        #region Coroutine

        /// <summary>
        /// 叠加加载其他场景时调用
        /// </summary>
        IEnumerator OnAddtiveLoad(System.Action<Texture2D> onLoad)
        {
            // audioListener.enabled = false;
            var wait = new WaitForSeconds(0.02f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            for (int i = 1; i <= 10; i++)
            {
                canvasGroup.alpha = 1 - i / 10f;
                yield return wait;
            }

            yield return new WaitForEndOfFrame();

            onLoad?.Invoke(Data.BackgroundTex);
        }

        /// <summary>
        /// 叠加加载其他场景时调用
        /// </summary>
        IEnumerator OnAddtiveUnLoad()
        {
            // audioListener.enabled = true;
            var wait = new WaitForSeconds(0.02f);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            for (int i = 1; i <= 10; i++)
            {
                canvasGroup.alpha = i / 10f;
                yield return wait;
            }
        }

        private IEnumerator AsyncLoadAllMusicGroupCover()
        {
            var currentIndex = coverScroll.Index;
            var totalMusicGroupCount = MusicGroups.Count;
            // 把当前索引也算上，所以加载范围至少为1，最长为整个长度
            var maxLength = Mathf.Max(currentIndex + 1, totalMusicGroupCount - currentIndex);
            string musicGroupName;
            for (int i = 0, index = 0; i < maxLength; i++)
            {
                // 先加载前边一个
                index = currentIndex - i;
                if (index >= 0)
                {
                    musicGroupName = MusicGroups[index].name;
                    if (!ImageLoader.GlobalImageLoader.IsResourceLoaded(musicGroupName))
                    {
                        var clip = ImageLoader.GlobalImageLoader.LoadResourceAsync(musicGroupName);
                        yield return new WaitUntil(() => clip.isDone);
                        ImageLoader.GlobalImageLoader.AddResource(musicGroupName, clip.asset as Texture2D);
                        Logger.Log($"{musicGroupName} Cover loaded!", Color.yellow);
                    }
                }

                // 再加载后边一个
                index = currentIndex + i;
                if (index < totalMusicGroupCount)
                {
                    musicGroupName = MusicGroups[index].name;
                    if (!ImageLoader.GlobalImageLoader.IsResourceLoaded(musicGroupName))
                    {
                        var clip = ImageLoader.GlobalImageLoader.LoadResourceAsync(musicGroupName);
                        yield return new WaitUntil(() => clip.isDone);
                        ImageLoader.GlobalImageLoader.AddResource(musicGroupName, clip.asset as Texture2D);
                        Logger.Log($"{musicGroupName} Cover loaded!", Color.yellow);
                    }
                }
            }
        }

        #endregion
    }
}