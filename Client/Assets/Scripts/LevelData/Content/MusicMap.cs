using System;
using System.Collections.Generic;
using Resux.LevelData;

namespace Resux
{
    /// <summary>
    /// 谱面
    /// </summary>
    public class MusicMap
    {
        /// <summary>谱面难度</summary>
        public Difficulty difficulty;
        /// <summary>难度定级</summary>
        public int diffLevel;

        // 谱面详情

        /// <summary>判定点列表</summary>
        public List<JudgeNoteInfo> judgeNotes;

        /// <summary>装饰性半note</summary>
        public List<NoteInfo> decorativeNotes;

        public MusicMap()
        {
            diffLevel = default;
            difficulty = default;
            judgeNotes = new List<JudgeNoteInfo>();
            decorativeNotes = new List<NoteInfo>();
        }

        public MusicMap(int diffLevel, Difficulty difficulty, List<JudgeNoteInfo> judgeNotes, List<NoteInfo> decorativeNotes)
        {
            this.difficulty = difficulty;
            this.diffLevel = diffLevel;
            this.judgeNotes = judgeNotes ?? throw new ArgumentNullException(nameof(judgeNotes));
            this.decorativeNotes = decorativeNotes ?? new List<NoteInfo>();
        }
    }
}