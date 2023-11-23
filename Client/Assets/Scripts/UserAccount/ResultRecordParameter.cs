namespace Resux
{
    /// <summary>
    /// 传递打歌结果记录的参数
    /// </summary>
    public class ResultRecordParameter
    {
        /// <summary>大p数量</summary>
        public int BigPerfect { get; private set; }

        /// <summary>小p数量</summary>
        public int Perfect { get; private set; }

        /// <summary>good数量</summary>
        public int Good { get; private set; }

        /// <summary>bad数量</summary>
        public int Bad { get; private set; }

        /// <summary>miss数量</summary>
        public int Miss { get; private set; }

        /// <summary>最大连击数</summary>
        public int MaxCombo { get; private set; }

        /// <summary>得分</summary>
        public int Score { get; private set; }

        /// <summary>成绩</summary>
        public ScoreGrade Grade { get; private set; }

        /// <summary>早P数量</summary>
        public int EarlyPerfectCount { get; private set; }

        /// <summary>晚P数量</summary>
        public int LatePerfectCount { get; private set; }

        public ScoreType ScoreType { get; private set; }

        public ResultRecordParameter(int bigBigPerfect, int perfect, int good, int bad, int miss,
            int maxCombo, int score, ScoreGrade scoreGrade, int earlyCount, int lateCount,
            ScoreType scoreType)
        {
            BigPerfect = bigBigPerfect;
            Perfect = perfect;
            Good = good;
            Bad = bad;
            Miss = miss;
            MaxCombo = maxCombo;
            Score = score;
            Grade = scoreGrade;
            EarlyPerfectCount = earlyCount;
            LatePerfectCount = lateCount;
            ScoreType = scoreType;
        }
    }
}