using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HalfNotePanelView : BaseView
{
    #region Properties

    [SerializeField] private Button alignButton;
    [SerializeField] private Toggle speedModeToggle;

    #region UpHalf

    [Space]
    [SerializeField] private HalfNoteEditView upHalfEditView;

    private EditNoteInfo upNote;

    #endregion

    #region DownHalf

    [Space]

    [SerializeField] private HalfNoteEditView downHalfEditView;

    private EditNoteInfo downNote;

    #endregion

    #endregion

    #region Unity

    private void OnDisable()
    {
        // SaveEditingNote();
    }

    #endregion

    #region Public Method

    public override void Initialize()
    {
        alignButton.onClick.AddListener(AlignToJudge);
        speedModeToggle.onValueChanged.AddListener(ChangeSpeedEditMode);

        upHalfEditView.Initialize();
        downHalfEditView.Initialize();
    }

    public override void UpdateView()
    {
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            Popup.ShowMessage("没有正在编辑的判定", Color.red);
            return;
        }

        EditingData.CurrentEditStep = EditPanelView.EditStep.HalfNote;
        upNote = EditingData.CurrentEditingJudgeNote.movingNotes.up;
        downNote = EditingData.CurrentEditingJudgeNote.movingNotes.down;
        upHalfEditView.SetHalfNote(upNote);
        downHalfEditView.SetHalfNote(downNote);
        upHalfEditView.UpdateView();
        downHalfEditView.UpdateView();
    }

    #endregion

    #region Private Method

    private void ChangeSpeedEditMode(bool isInputMode)
    {
        upHalfEditView.ChangeSpeedEditMode(isInputMode);
        downHalfEditView.ChangeSpeedEditMode(isInputMode);
    }

    /// <summary>
    /// 将上下半note对齐到判定
    /// </summary>
    private void AlignToJudge()
    {
        var judge = EditingData.CurrentEditingJudgeNote.judge;
        // 检查路径是否能对齐判定点
        if (!Tools.IsInPath(judge.judgePosition.y, upNote.path, pos => pos.y) && !CheckCanAlignInWater(upNote, judge))
        {
            Popup.ShowMessage("请确保上半note路径y轴可以经过判定点 或 完全在水下！", Color.red);
            return;
        }
        else if (!Tools.IsInPath(judge.judgePosition.y, downNote.path, pos => pos.y) && !CheckCanAlignInWater(downNote, judge))
        {
            Popup.ShowMessage("请确保下半note路径y轴可以经过判定点 或 完全在水下！", Color.red);
            return;
        }
        // 上半
        AlignToJudge(upNote, judge);
        // 下半
        AlignToJudge(downNote, judge);

        UpdateView();
    }

    private void AlignToJudge(EditNoteInfo halfNote, JudgePair judge)
    {
        // 查找到路径中恰好穿过水面的两个点（水面上一个水面下一个）
        var waterFacePointIndex = 0;
        for (int i = 0; i < halfNote.path.Length - 1; i++)
        {
            if (Tools.IsInArea(ConstData.WaterFaceHeight, halfNote.path[i].y, halfNote.path[i+1].y))
            {
                waterFacePointIndex = i;
                break;
            }
        }
        // 纵向平移的可行区间
        // 两者一正一负，向上平移用正值，向下平移用负值
         (float verticalOffset1, float verticalOffset2) = (halfNote.path[waterFacePointIndex].y - ConstData.WaterFaceHeight,
            halfNote.path[waterFacePointIndex + 1].y - ConstData.WaterFaceHeight);
        // 最接近判定点位置的点索引
        var closedJudgePointIndex = 0;
        var distanceFromJudge = float.MaxValue;
        List<(float distance, int index)> distanceTuples = new List<(float distance, int index)>();
        
        if (halfNote.noteMoveInfo.isReverse)
        {
            // 浮现需要反过来，否则绝大部分点都算不进去
            for (int i = halfNote.path.Length - 1; i >= 0; i--)
            {
                var dis = Mathf.Abs(halfNote.path[i].y - judge.judgePosition.y);
                if (distanceFromJudge > dis)
                {
                    distanceFromJudge = dis;
                    closedJudgePointIndex = i;
                    distanceTuples.Add((dis, i));
                }
            }
        }
        else
        {
            for (int i = 0; i < halfNote.path.Length; i++)
            {
                var dis = Mathf.Abs(halfNote.path[i].y - judge.judgePosition.y);
                if (distanceFromJudge > dis)
                {
                    distanceFromJudge = dis;
                    closedJudgePointIndex = i;
                    distanceTuples.Add((dis, i));
                }
            }
        }

        // 升序，然后取最靠近尾部的
        distanceTuples.Sort((left, right) => left.distance.CompareTo(right.distance));
        closedJudgePointIndex = distanceTuples[0].index;
        for (int i = 1; i < 10; i++)
        {
            if (closedJudgePointIndex < distanceTuples[i].index)
            {
                closedJudgePointIndex = distanceTuples[i].index;
            }
        }

        // 开始对齐
        var alignPos = new Vector2();
        // 水平对齐
        var offset = judge.judgePosition.x - halfNote.path[closedJudgePointIndex].x;
        alignPos.x = halfNote.noteMoveInfo.p0.x + offset;
        // 竖直对齐
        distanceFromJudge = judge.judgePosition.y - halfNote.path[closedJudgePointIndex].y;
        // 尽量接近判定点，但是没法完美所以没办法
        {
            float up, down;
            if (verticalOffset1 < 0)
            {
                up = verticalOffset2;
                down = verticalOffset1;
            }
            else
            {
                up = verticalOffset1;
                down = verticalOffset2;
            }
            // 这说明整个轨迹都在水里，就只有一个向上移动的限制了
            if (waterFacePointIndex == 0)
            {
                var maxOffset = halfNote.path.Min(pos => ConstData.WaterFaceHeight - pos.y);
                alignPos.y = halfNote.noteMoveInfo.p0.y + Mathf.Min(distanceFromJudge, maxOffset);
            }
            else
            {
                // >0向上平移
                if (distanceFromJudge > 0)
                {
                    alignPos.y = halfNote.noteMoveInfo.p0.y + Mathf.Min(up, distanceFromJudge);
                }
                else
                {
                    alignPos.y = halfNote.noteMoveInfo.p0.y + Mathf.Max(down, distanceFromJudge);
                }
            }
        }

        // 时间对齐
        if (halfNote.noteMoveInfo.isReverse)
        {
            halfNote.noteMoveInfo.startTime = judge.judgeTime;
            halfNote.noteMoveInfo.endTime = judge.judgeTime + (halfNote.path.Length - 1 - closedJudgePointIndex) * 10;
        }
        else
        {
            halfNote.noteMoveInfo.endTime = judge.judgeTime;
            halfNote.noteMoveInfo.startTime = judge.judgeTime - closedJudgePointIndex * 10;
        }
        // 位置最后再对齐，因为会调用路径计算方法
        halfNote.SetInitPos(alignPos);
    }

    /// <summary>
    /// 检查y不经过判定点的半note是否可以对齐
    /// 如果可以对齐，说明整个路径都在水下
    /// </summary>
    /// <param name="halfNote"></param>
    /// <param name="judge"></param>
    /// <returns>可否对齐</returns>
    private bool CheckCanAlignInWater(EditNoteInfo halfNote, JudgePair judge)
    {
        var distanceFromWaterFace = halfNote.path.Min(pos => Mathf.Abs(ConstData.WaterFaceHeight - pos.y));
        var distanceFromJudgePos = halfNote.path.Min(pos => Mathf.Abs(judge.judgePosition.y - pos.y));

        return distanceFromWaterFace > distanceFromJudgePos;
    }

    #endregion
}
