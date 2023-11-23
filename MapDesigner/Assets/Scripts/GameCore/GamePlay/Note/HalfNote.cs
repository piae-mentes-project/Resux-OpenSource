using Resux.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.GamePlay
{
    /// <summary>
    /// 半note
    /// </summary>
    public class HalfNote
    {
        #region static

        public static AnimationCurve alphaCurve;
        public static ObjectPool<Image> notePool;
        public static ObjectPool<HoldLineRenderer> holdLinePool;

        public static Dictionary<HalfType, Dictionary<JudgeType, Sprite>> SpriteDic;
        private static Material MultiPressMaterial;
        private static Material DefaultMaterial;

        #endregion

        #region const

        private const int preShowHoldTime = 900;

        #endregion

        #region properties

        /// <summary>note基本信息</summary>
        private NoteInfo info;
        /// <summary>对应的判定类型</summary>
        public JudgeType judgeType { get; private set; }

        /// <summary>hold 路径</summary>
        private List<Vector2> holdPath => info.holdPath;

        private int holdPosIndex;

        /// <summary>
        /// note所处的时间（整数采样），单位毫秒
        /// 用于更新位置
        /// </summary>
        private int time;
        /// <summary>
        /// 出现的时间
        /// </summary>
        public int appearTime => info.noteMoveInfo.startTime;
        /// <summary>
        /// 两个note碰撞的时间
        /// </summary>
        public int collisionTime => info.noteMoveInfo.endTime;

        /// <summary>
        /// 消失的时间
        /// </summary>
        private int disappearTime;

        /// <summary>
        /// 移动轨迹
        /// </summary>
        private Vector2[] posList = null;

        private int pathLengthWithoutHold;

        /// <summary>原始缩放大小</summary>
        private Vector3 rawScale;

        /// <summary>目标缩放大小</summary>
        private float scaleOffset;

        /// <summary>是否属于多押</summary>
        private bool isMulti;

        #region Public

        /// <summary>
        /// note物体
        /// </summary>
        public Image note;

        public HoldLineRenderer HoldPath { get; private set; }

        private bool showHoldPath;

        public event Action OnMoveEnd;

        #endregion

        #endregion

        static HalfNote()
        {
            SpriteDic = new Dictionary<HalfType, Dictionary<JudgeType, Sprite>>
            {
                {HalfType.UpHalf, new Dictionary<JudgeType, Sprite>()},
                {HalfType.DownHalf, new Dictionary<JudgeType, Sprite>()}
            };
            foreach (JudgeType type in Enum.GetValues(typeof(JudgeType)))
            {
                SpriteDic[HalfType.UpHalf].Add(type, Resources.Load<Sprite>($"Image/Note/{type}_Up"));
                SpriteDic[HalfType.DownHalf].Add(type, Resources.Load<Sprite>($"Image/Note/{type}_Down"));
            }

            // 多押和默认预制体
            MultiPressMaterial = Resources.Load<Material>("Materials/MultiPress");
            DefaultMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        public HalfNote(NoteInfo info, JudgeType judgeType, bool isMulti, bool showHoldPath)
        {
            this.info = info;
            this.judgeType = judgeType;
            this.isMulti = isMulti;
            this.showHoldPath = showHoldPath;

            CalculatePosList();

            disappearTime = info.noteMoveInfo.endTime + info.holdPath.Count * 10 + (int)(JudgeMethods.JudgeSetting[judgeType][JudgeResult.Bad].y + 0.5f);
            holdPosIndex = 0;
            scaleOffset = 0.3f;
        }

        #region Public Method

        public void Update(int t)
        {
            // 当没有note的时候说明还没显示过
            if (!note)
            {
                // 距离可以显示的时间
                if (t - appearTime + 100 > 0)
                {
                    GetNoteObject();
                }
                else
                {
                    return;
                }
            }

            // 有note了，如果超出时间那就需要回收了
            if (t > disappearTime)
            {
                if (note)
                {
                    HideNote();
                }

                return;
            }

            // 显示与否只与时间有关
            note.gameObject.SetActive(t >= appearTime && t <= disappearTime);

            time = t - appearTime;

            // 半note运动
            if (time <= 0)
            {
                var color = note.color;
                color.a = alphaCurve.Evaluate(0);
                note.color = color;
                return;
            }
            // time所在的区间
            var lastTime = time - time % 10;
            var lastFrame = lastTime / 10;
            var length = posList.Length;
            // 轨迹列表以从出现开始的时间偏移为索引，而不是实际时间
            if (lastFrame > 0 && lastFrame < length)
            {
                var lastPos = posList[lastFrame];
                var futureFrame = lastFrame + 1;
                var color = note.color;
                if (futureFrame > pathLengthWithoutHold)
                {
                    if (judgeType != JudgeType.Hold)
                    {
                        color.a = 1 - (futureFrame - pathLengthWithoutHold) / (float)(length - pathLengthWithoutHold);
                        note.color = color;
                    }
                }
                else
                {
                    color.a = alphaCurve.Evaluate(futureFrame / (float)pathLengthWithoutHold);
                    note.color = color;
                }

                if (futureFrame < length)
                {
                    var futurePos = posList[futureFrame];
                    var pos = Vector2.Lerp(lastPos, futurePos, (time % 10) / 10.0f);
                    note.transform.localPosition = pos;
                    return;
                }
                note.transform.localPosition = lastPos;
            }
        }

        /// <summary>
        /// 每0.01s更新
        /// </summary>
        /// <param name="t">当前所处的谱面时间</param>
        public void FixedUpdate(int t)
        {
            if (holdPath == null || holdPath.Count <= 0 || !showHoldPath)
            {
                return;
            }

            if (t > disappearTime)
            {
                return;
            }

            // 达到或超过了出现时间
            time = Mathf.Max(0, t - appearTime);

            // hold存在 且 距离碰撞的时间900ms前
            // 因为每10ms一个点所以需要用固定的物理帧
            if (t + preShowHoldTime >= collisionTime)
            {
                if (!HoldPath && judgeType == JudgeType.Hold)
                {
                    HoldPath = holdLinePool.GetObject();
                    HoldPath.transform.localPosition = Vector3.zero;
                    HoldPath.gameObject.SetActive(true);
                }

                if (holdPosIndex < holdPath.Count)
                {
                    HoldPath.AddPoint(holdPath[holdPosIndex]);
                    holdPosIndex++;
                }
            }
        }

        public void HideNoteByJudge()
        {
            if (note)
            {
                HideNote();
            }
        }

        /// <summary>
        /// 计算运动轨迹
        /// </summary>
        public void CalculatePosList()
        {
            posList = NoteMove.CalculatePositionList(info, appearTime, judgeType, out pathLengthWithoutHold);
        }

        /// <summary>
        /// 变小动画，hold用的
        /// </summary>
        public void ScaleSmall()
        {
            if (!note)
            {
                return;
            }

            var targetScale = note.transform.localScale;
            var minScale = rawScale * 0.8f;
            CoroutineUtils.StartCoroutine(CoroutineUtils.ExecutePerDelayTime(
                action: (count) =>
                {
                    if (null != note) note.transform.localScale = Vector3.Lerp(targetScale, minScale, (1 + count) / 5f);
                }, 0.016f, 5)
            );
        }

        public void MultiScale(float multi)
        {
            if (!note)
            {
                return;
            }

            if (multi < 0.00001f || multi > 0.99999f)
            {
                note.transform.localScale = rawScale * (1 + scaleOffset);
            }
            else
            {
                float addMulti;
                addMulti = (2.624f - 2.24f * multi) + 1 / (2.624f - 2.24f * multi) - 2;

                note.transform.localScale = rawScale * (1 + addMulti * scaleOffset);
            }
        }

        /// <summary>
        /// 变大动画，hold用的
        /// </summary>
        public void ScaleLarge()
        {
            if (!note)
            {
                return;
            }

            rawScale = note.transform.localScale;
            scaleOffset = 0.3f;
            var targetScale = rawScale * (1 + scaleOffset);
            CoroutineUtils.StartCoroutine(CoroutineUtils.ExecutePerDelayTime(
                action: (count) =>
                {
                    note.transform.localScale = Vector3.Lerp(rawScale, targetScale, (1 + count) / 5f);
                }, 0.016f, 5)
            );
        }

        /// <summary>
        /// 大->小->大
        /// </summary>
        public void ScaleJudge()
        {
            if (!note)
            {
                return;
            }

            var targetScale = rawScale * (1 + scaleOffset);
            CoroutineUtils.StartCoroutine(CoroutineUtils.ExecutePerDelayTime(
                action: (count) =>
                {
                    if (count < 5)
                    {
                        note.transform.localScale = Vector3.Lerp(targetScale, rawScale, (1 + count) / 5f);
                    }
                    else
                    {
                        note.transform.localScale = Vector3.Lerp(rawScale, targetScale, (1 + count - 5) / 5f);
                    }
                }, 0.01f, 10)
            );
        }

        public void Reset(int currentTime)
        {
            RefreshHoldPath(currentTime);
        }

        public void RefreshHoldPath(int currentTime)
        {
            if (holdPath == null || holdPath.Count <= 0)
            {
                return;
            }

            var rawIndex = (currentTime + preShowHoldTime - collisionTime) / 10;
            holdPosIndex = Mathf.Max(rawIndex, 0);
            
            if (rawIndex < 0 && HoldPath || rawIndex >= holdPath.Count)
            {
                holdLinePool.ReturnToPool(HoldPath);
                HoldPath = null;
            }
            else if (HoldPath)
            {
                HoldPath.Initialize();
                for (int i = 0; i < rawIndex - 1; i++)
                {
                    HoldPath.AddPoint(holdPath[i]);
                }
            }
        }

        #endregion

        #region Private Method

        private void GetNoteObject()
        {
            note = notePool.GetObject();
            note.sprite = SpriteDic[info.halfType][judgeType];
            note.transform.localPosition = posList[0];
            note.material = isMulti ? MultiPressMaterial : DefaultMaterial;

            rawScale = note.transform.localScale;
        }

        private void HideNote()
        {
            note.transform.localScale = rawScale;
            notePool.ReturnToPool(note);
            note = null;
            if (HoldPath)
            {
                holdLinePool.ReturnToPool(HoldPath);
                HoldPath = null;
            }

            OnMoveEnd?.Invoke();
        }

        #endregion
    }
}
