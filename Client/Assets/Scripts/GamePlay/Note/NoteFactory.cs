using UnityEngine;
using System.Collections;
using Resux.Component;
using Resux.LevelData;

namespace Resux.GamePlay
{
    /// <summary>
    /// 创建note的工厂
    /// </summary>
    public static class NoteFactory
    {
        #region properties

        private const string NotePrefabPath = "Prefabs/Notes";

        /// <summary>note预制体</summary>
        public static GameObject NotePrefab { get; private set; }
        public static GameObject HoldPathPrefab { get; private set; }
        public static GameObject JudgePoint { get; private set; }

        #endregion

        static NoteFactory()
        {
            NotePrefab = Resources.Load<GameObject>($"{NotePrefabPath}/Note");
            HoldPathPrefab = Resources.Load<GameObject>($"{NotePrefabPath}/HoldPath");
            JudgePoint = Resources.Load<GameObject>($"{NotePrefabPath}/JudgePoint");
        }

        #region 创建方法

        /// <summary>
        /// 创建装饰性note
        /// </summary>
        /// <param name="noteInfo"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static HalfNote CreateDecorativeNote(NoteInfo noteInfo, Transform parent)
        {
            var note = GameObject.Instantiate(NotePrefab, parent);
            note.transform.position = UI.ScreenAdaptation.AdaptationNotePosition(noteInfo.noteMoveInfo.p0);

            var halfNote = new HalfNote(noteInfo, JudgeType.Tap, false, true);
            return halfNote;
        }

        public static SpriteRenderer CreareHalfNote(Transform parent)
        {
            var note = GameObject.Instantiate(NotePrefab, parent);

            return note.GetComponent<SpriteRenderer>();
        }

        public static HoldLineRenderer CreateHoldPath(Transform parent)
        {
            var holdPath = GameObject.Instantiate(HoldPathPrefab, parent).GetComponent<HoldLineRenderer>();
            holdPath.Initialize();
            return holdPath;
        }

        public static GameObject CreateJudgePoint(Transform parent)
        {
            var judgePoint = GameObject.Instantiate(JudgePoint, parent);
            judgePoint.SetActive(false);

            return judgePoint;
        }

        #endregion
    }
}