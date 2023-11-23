using System;
using System.Collections.Generic;
using Resux.LevelData;

namespace Resux
{
    /// <summary>
    /// 曲目合集信息
    /// </summary>
    [Serializable]
    public class ChapterDetail
    {
        public int id;
        public string name;

        public List<LevelData.LevelDetail> musicConfigs;
    }
}