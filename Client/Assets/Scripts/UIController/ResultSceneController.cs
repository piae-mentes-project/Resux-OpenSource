using System;
using System.Collections;
using System.Text;
using Resux.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Resux.UI;
using Resux.Data;
using Resux.GamePlay;
using Resux.Manager;

namespace Resux.UI.Manager
{
    [Serializable]
    public class RecordUIPack
    {
        public Text Perfect;
        public Text Good;
        public Text Bad;
        public Text Miss;
        public Text OnlyPerfect;
    }

    /// <summary>
    /// 结算页UI管理
    /// </summary>
    public class ResultSceneController : MonoBehaviour
    {
        #region inner class

        public static class Data
        {
            /// <summary>成绩</summary>
            public static ResultRecordParameter ResultRecord;
            /// <summary>新纪录</summary>
            public static bool IsNewBest;
            /// <summary>和历史记录的分差</summary>
            public static int ScoreDifference;
        }

        #endregion

        #region properties

        public static GameScene ThisScene => GameScene.ResultScene;

        [SerializeField] private Text titleText;
        [Header("打歌记录信息")]
        /// <summary>最大连击数</summary>
        [SerializeField] private Text maxCombo;
        /// <summary>得分</summary>
        [SerializeField] private Text score;
        [SerializeField] private Text scoreChange;
        /// <summary>记录</summary>
        [SerializeField] private RecordUIPack recordPack;
        /// <summary>结果图（比如φ了）</summary>
        [SerializeField] private Image result;

        [Header("歌曲信息")]
        /// <summary>曲绘</summary>
        [SerializeField] private RawImage musicCover;
        /// <summary>歌名</summary>
        [SerializeField] private Text musicName;
        /// <summary>难度</summary>
        [SerializeField] private Text musicDifficulty;

        [Header("玩家信息")]
        [SerializeField] private Image playerAvatar;
        [SerializeField] private Text playerName;

        [Header("交互操作UI")]
        [SerializeField] private Button okButton;
        [SerializeField] private Text okBtnLabel;
        /// <summary>分享按钮</summary>
        [SerializeField] private Button shareButton;
        // [SerializeField] private Text shareBtnLabel;
        /// <summary>重试按钮</summary>
        [SerializeField] private Button retryButton;
        [SerializeField] private Text retryBtnLabel;

        #endregion

        #region UnityEngine

        void OnEnable()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            // 保存
            PlayerRecordManager.SaveRecord();
            MusicScoreRecorder.Instance.Reset();
            AudioPlayManager.Instance.StopBGM();
        }

        #endregion

        #region Public Method

        public void Initialize()
        {
            InitResult();

            okButton.onClick.AddListener(ReturnToMain);
            retryButton.onClick.AddListener(PlayAgain);
            shareButton.onClick.AddListener(OnShare);

            // 玩家信息
            var user = AccountManager.User;
            playerName.text = user.NickName;
            playerAvatar.sprite = user.Avatar;

            AudioPlayManager.Instance.PlayBGM(Sounds.Bgm.ResultScene);
        }

        /// <summary>
        /// 设置结算界面
        /// </summary>
        public void InitResult()
        {
            var resultRecord = Data.ResultRecord;
            // 成绩
            maxCombo.text = resultRecord.MaxCombo.ToString();
            score.text = Utils.GetScoreText(resultRecord.Score, new Color(1, 1, 1, 0.25f), Color.white);
            var offset = Data.ScoreDifference;
            scoreChange.text = offset < 0 ? offset.ToString() : $"+{offset}";
            recordPack.Perfect.text = $"{resultRecord.Perfect + resultRecord.BigPerfect}";
            recordPack.OnlyPerfect.text = resultRecord.Perfect > 0 ? $"-{resultRecord.EarlyPerfectCount}E / -{resultRecord.LatePerfectCount}L" : "";
            recordPack.Good.text = resultRecord.Good.ToString();
            recordPack.Bad.text = resultRecord.Bad.ToString();
            recordPack.Miss.text = resultRecord.Miss.ToString();
            var gradeSprite = Resources.Load<Sprite>($"Image/Grade/{resultRecord.Grade}");
            result.sprite = gradeSprite;

            // 信息
            var musicConfig = MusicScoreSelection.LevelDetail;
            musicName.text = musicConfig._songName;
            musicCover.texture = Assets.ImageLoader.GetMusicCover(musicConfig._songName);

            musicDifficulty.text = $"[[{MusicScoreSelection.Difficulty.GetName()}]] {musicConfig.GetLevelStr(MusicScoreSelection.Difficulty)}";

            if (MusicScoreRecorder.Instance.IsBanned)
            {
                // 检测到作弊，要向服务器发送消息并强制返回标题
                // TODO: 发送作弊信息到服务器
                PopupView.Instance.ShowUniversalWindow("发生异常，返回标题界面", PopupType.Normal,
                    onCancel: null,
                    onOk: TurnToTitle);
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 返回主场景
        /// </summary>
        private void ReturnToMain()
        {
            Navigator.JumpScene(GameScene.MusicListScene);
        }

        /// <summary>
        /// 返回标题画面
        /// </summary>
        private void TurnToTitle()
        {
            Navigator.ClearEscapeStack();
            Navigator.JumpScene(GameScene.EntryScene);
        }

        private void PlayAgain()
        {
            AudioPlayManager.Instance.PlayUIEffect(Sounds.Effect.StartPlay);
            Navigator.JumpScene(GameScene.PlayScene);
        }

        private void OnShare()
        {

        }

        #endregion
    }
}