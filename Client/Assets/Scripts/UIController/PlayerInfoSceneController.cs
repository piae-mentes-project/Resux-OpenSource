using System.Collections;
using Resux.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Resux.Data;
using Resux.LevelData;
using Resux.Manager;

namespace Resux.UI.Manager
{
    /// <summary>
    /// 玩家个人信息界面
    /// </summary>
    public class PlayerInfoSceneController : MonoBehaviour
    {
        #region properties

        public static GameScene ThisScene => GameScene.PlayerInfoScene;

        #region Scene Object

        #region Account

        [SerializeField] private Button backButton;
        [SerializeField] private Image avatar;
        [SerializeField] private Text uid;
        [SerializeField] private Text nickNameLabel;
        [SerializeField] private Text nickName;

        #endregion

        #region Record

        [Space(10)]
        [SerializeField] private Text[] taleDifficultyTexts;
        [SerializeField] private Text[] romanceDifficultyTexts;
        [SerializeField] private Text[] historyDifficultyTexts;


        [Space(10)]
        [SerializeField] private Text taleClearCount;
        [SerializeField] private Text romanceClearCount;
        [SerializeField] private Text historyClearCount;
        [SerializeField] private Text totalClearCount;

        [Space(10)]
        [SerializeField] private Text taleFcCount;
        [SerializeField] private Text romanceFcCount;
        [SerializeField] private Text historyFcCount;
        [SerializeField] private Text totalFcCount;

        [Space(10)]
        [SerializeField] private Text taleApCount;
        [SerializeField] private Text romanceApCount;
        [SerializeField] private Text historyApCount;
        [SerializeField] private Text totalApCount;

        [Space(10)]
        [SerializeField] private Text taleTheoryCount;
        [SerializeField] private Text romanceTheoryCount;
        [SerializeField] private Text historyTheoryCount;
        [SerializeField] private Text totalTheoryCount;

        #endregion

        #endregion

        #endregion

        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // user info
            avatar.sprite = Resources.Load<Sprite>($"{Assets.AssetsFilePathDM.UniversalUIAssetsPath_Assets}/Account");
            uid.text = AccountManager.User.Uid.ToString();
            nickNameLabel.text = $"{"NICKNAME".Localize()}: ";
            nickName.text = AccountManager.User.NickName;
            backButton.onClick.AddListener(GlobalStaticUIManager.Instance.BackToPrevScene);

            // 难度名初始化
            foreach (var difficultyName in taleDifficultyTexts)
            {
                difficultyName.text = Difficulty.Tale.GetName();
            }
            foreach (var difficultyName in romanceDifficultyTexts)
            {
                difficultyName.text = Difficulty.Romance.GetName();
            }
            foreach (var difficultyName in historyDifficultyTexts)
            {
                difficultyName.text = Difficulty.History.GetName();
            }

            var count = MusicDataDM.MusicCount;
            // clear record
            taleClearCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Tale)}/{count}";
            romanceClearCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Romance)}/{count}";
            historyClearCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.History)}/{count}";
            totalClearCount.text = $"{PlayerInfoManager.ClearCount}/{count}";
            // fc
            taleFcCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Tale, ScoreType.FullCombo)}/{count}";
            romanceFcCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Romance, ScoreType.FullCombo)}/{count}";
            historyFcCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.History, ScoreType.FullCombo)}/{count}";
            totalFcCount.text = $"{PlayerInfoManager.FullComboCount}/{count}";
            // ap
            taleApCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Tale, ScoreType.AllPerfect)}/{count}";
            romanceApCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Romance, ScoreType.AllPerfect)}/{count}";
            historyApCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.History, ScoreType.AllPerfect)}/{count}";
            totalApCount.text = $"{PlayerInfoManager.AllPerfectCount}/{count}";
            // 理论
            taleTheoryCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Tale, ScoreType.Theory)}/{count}";
            romanceTheoryCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.Romance, ScoreType.Theory)}/{count}";
            historyTheoryCount.text = $"{PlayerInfoManager.GetRecordCount(Difficulty.History, ScoreType.Theory)}/{count}";
            totalTheoryCount.text = $"{PlayerInfoManager.TheoryCount}/{count}";
        }
    }
}