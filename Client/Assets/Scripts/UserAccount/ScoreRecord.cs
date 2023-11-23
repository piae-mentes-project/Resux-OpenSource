using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resux.LevelData;

namespace Resux
{
    /// <summary>
    /// 打歌结果记录（可序列化）
    /// </summary>
    [Serializable]
    public class ScoreRecord
    {
        public int MusicId = 0;
        public Difficulty Difficulty;
        public int Score = 0;
        public ScoreType ScoreType;
        public ScoreType BestScoreType;
        public ScoreGrade ScoreGrade
        {
            get
            {
                if (ScoreType == ScoreType.Theory)
                {
                    return ScoreGrade.Maximum;
                }

                return GameConfigs.GetGrade(Score);
            }
        }
    }

    ///<summary>结算时的打歌结果</summary>
    public enum ScoreType
    {
        ///<summary>没死，但是您不起来</summary>
        Clear,
        /// <summary>
        /// Full Combo
        /// （即combo == 总note数）
        /// </summary>
        FullCombo,
        /// <summary>
        /// All Perfect
        /// （即score >= 1,000,000）
        /// </summary>
        AllPerfect,
        ///<summary>您理论了</summary>
        Theory
    }

    /// <summary>
    /// 评分对应的结果，相当于考试时的分数线划分（即A+92分，A88分，B+...分等）
    /// </summary>
    public enum ScoreGrade
    {
        /// <summary>
        /// 理论值
        ///  （即score == 1,000,000 + 该关卡的Note数量）
        /// </summary>
        Maximum,
        /// <summary>900,000以上</summary>
        Outstanding,
        /// <summary>850,000~899,999</summary>
        ExceedExpectations,
        /// <summary>800,000~849,999</summary>
        Acceptable,
        /// <summary>700,000~799,999</summary>
        Poor,
        /// <summary>0~699,999</summary>
        Dreadful,
    }
}
