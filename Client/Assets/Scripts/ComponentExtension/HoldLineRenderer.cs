using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Component
{
    /// <summary>
    /// 针对hold的LineRenderer优化
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class HoldLineRenderer : MonoBehaviour
    {
        #region properties

        private LineRenderer lineRenderer;
        private LineRenderer LineRenderer
        {
            get
            {
                if (lineRenderer == null)
                {
                    lineRenderer = GetComponent<LineRenderer>();
                }

                return lineRenderer;
            }
        }

        public int Count { get; private set; }
        private int positionCount;
        private int currentIndex;
        private Vector2? lastPos;
        private Vector2? currentPos;
        [SerializeField]
        [Tooltip("合并线段的斜率夹角阈值")]
        [Range(0, 10)]
        private float angleThreshold = 1;
        [SerializeField]
        [Tooltip("大拐角后追加点数")]
        [Range(0, 10)]
        private int addCountThreshold = 4;
        [SerializeField]
        [Tooltip("直线自优化开始的最大点数")]
        [Range(1, 10)]
        private int maxOptimizationCount = 4;
        private List<Vector2> points;

        public float Width
        {
            get => LineRenderer.widthMultiplier;
            set => LineRenderer.widthMultiplier = value;
        }

        /// <summary>是否需要追加点位（在优化直线之前）</summary>
        private bool needAddPoint;
        /// <summary>追加的点位数量</summary>
        private int addCount;

        #endregion

        #region Public Method

        public void Initialize()
        {
            LineRenderer.positionCount = 0;
            points = new List<Vector2>(maxOptimizationCount);
            addCount = Count = positionCount = currentIndex = 0;
            needAddPoint = false;
        }

        public void AddPoint(Vector2 pos)
        {
            if (LineRenderer.positionCount < 2)
            {
                currentIndex = positionCount;
                LineRenderer.positionCount = ++positionCount;
                LineRenderer.SetPosition(currentIndex, pos);
                if (lastPos.HasValue)
                {
                    currentPos = pos;
                }
                else
                {
                    lastPos = pos;
                }
            }
            else
            {
                var dir1 = currentPos.Value - lastPos.Value;
                var dir2 = pos - currentPos.Value;
                // 前后线段的角度
                var angle = Mathf.Abs(Vector2.Angle(dir1, dir2)) % 360;

                // 重置计数
                needAddPoint = angle > 45;
                if (needAddPoint)
                {
                    addCount = 0;
                }

                needAddPoint = needAddPoint || (addCount > 0 && addCount < addCountThreshold);
                // 可以进行优化
                if (angle < angleThreshold && !needAddPoint)
                {
                    addCount = 0;
                    // 需要添加点
                    if (points.Count < maxOptimizationCount)
                    {
                        points.Add(pos);
                        currentIndex = positionCount;
                        LineRenderer.positionCount = ++positionCount;
                        lastPos = currentPos;
                        LineRenderer.SetPosition(currentIndex, pos);
                    }
                    else
                    {
                        // 需要points的所有点都前移
                        points.RemoveAt(0);
                        points.Add(pos);
                        for (int i = 0; i < maxOptimizationCount; i++)
                        {
                            // 倒数第 max - i - 1个，也就是倒数max-1到0。因为count=maxindex+1
                            LineRenderer.SetPosition(positionCount - maxOptimizationCount + i, points[i]);
                        }

                        // lastPos = currentPos;
                    }

                    currentPos = pos;
                }
                else
                {
                    currentIndex = positionCount;
                    LineRenderer.positionCount = ++positionCount;
                    LineRenderer.SetPosition(currentIndex, pos);
                    lastPos = currentPos;
                    currentPos = pos;
                    if (needAddPoint)
                    {
                        addCount++;
                    }

                    // 拐点，清空points
                    points.Clear();
                }
            }
        }

        #endregion
    }
}
