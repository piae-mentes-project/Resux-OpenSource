using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Resux.LevelData
{
    [Serializable]
    public class LevelDetail
    {
        public int _id;
        public string _songName;
        public string _subTitle;
        public string _artistName;
        public string _illustrationName;
        public int _musicGroupId;
        public ChartDetail[] _chartInfos;
        public Vector2Int bpmRange;
        public Vector2Int musicPreviewRange;
        public bool isUnLocked = true;

        public string GetLevelStr(Difficulty difficulty) => GetLevelStr((int)difficulty);
        public string GetLevelStr(int difficulty)
        {
            var level = _chartInfos[difficulty]._level;
            string percentLv = "0";
            if (level >= 100)
            {
                percentLv = (level / 10).ToString();
            }
            else
            {
                percentLv = (level / 10f).ToString();
                if (percentLv.Length > 3)
                {
                    percentLv = percentLv.Substring(0, 3);
                }
            }

            return $"{percentLv}%";
        }

        public string GetLevelDesigner(Difficulty difficulty) => GetLevelDesigner((int)difficulty);
        public string GetLevelDesigner(int difficulty) => _chartInfos[difficulty]._designer;

        public bool ContainsDifficulty(Difficulty difficulty) => ContainsDifficulty((int)difficulty);
        public bool ContainsDifficulty(int difficulty)
        {
            if (_chartInfos.Length <= difficulty)
            {
                return false;
            }

            return _chartInfos[difficulty]._level > 0 && _chartInfos[difficulty]._level < 1000;
        }
    }

    /// <summary>歌曲难度</summary>
    public enum Difficulty
    {
        Tale,
        Romance,
        History,
        Story,
        Revival,
    }
}
