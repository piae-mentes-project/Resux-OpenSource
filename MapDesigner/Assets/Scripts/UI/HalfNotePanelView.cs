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
            Popup.ShowMessage("û�����ڱ༭���ж�", Color.red);
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
    /// �����°�note���뵽�ж�
    /// </summary>
    private void AlignToJudge()
    {
        var judge = EditingData.CurrentEditingJudgeNote.judge;
        // ���·���Ƿ��ܶ����ж���
        if (!Tools.IsInPath(judge.judgePosition.y, upNote.path, pos => pos.y) && !CheckCanAlignInWater(upNote, judge))
        {
            Popup.ShowMessage("��ȷ���ϰ�note·��y����Ծ����ж��� �� ��ȫ��ˮ�£�", Color.red);
            return;
        }
        else if (!Tools.IsInPath(judge.judgePosition.y, downNote.path, pos => pos.y) && !CheckCanAlignInWater(downNote, judge))
        {
            Popup.ShowMessage("��ȷ���°�note·��y����Ծ����ж��� �� ��ȫ��ˮ�£�", Color.red);
            return;
        }
        // �ϰ�
        AlignToJudge(upNote, judge);
        // �°�
        AlignToJudge(downNote, judge);

        UpdateView();
    }

    private void AlignToJudge(EditNoteInfo halfNote, JudgePair judge)
    {
        // ���ҵ�·����ǡ�ô���ˮ��������㣨ˮ����һ��ˮ����һ����
        var waterFacePointIndex = 0;
        for (int i = 0; i < halfNote.path.Length - 1; i++)
        {
            if (Tools.IsInArea(ConstData.WaterFaceHeight, halfNote.path[i].y, halfNote.path[i+1].y))
            {
                waterFacePointIndex = i;
                break;
            }
        }
        // ����ƽ�ƵĿ�������
        // ����һ��һ��������ƽ������ֵ������ƽ���ø�ֵ
         (float verticalOffset1, float verticalOffset2) = (halfNote.path[waterFacePointIndex].y - ConstData.WaterFaceHeight,
            halfNote.path[waterFacePointIndex + 1].y - ConstData.WaterFaceHeight);
        // ��ӽ��ж���λ�õĵ�����
        var closedJudgePointIndex = 0;
        var distanceFromJudge = float.MaxValue;
        List<(float distance, int index)> distanceTuples = new List<(float distance, int index)>();
        
        if (halfNote.noteMoveInfo.isReverse)
        {
            // ������Ҫ��������������󲿷ֵ㶼�㲻��ȥ
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

        // ����Ȼ��ȡ���β����
        distanceTuples.Sort((left, right) => left.distance.CompareTo(right.distance));
        closedJudgePointIndex = distanceTuples[0].index;
        for (int i = 1; i < 10; i++)
        {
            if (closedJudgePointIndex < distanceTuples[i].index)
            {
                closedJudgePointIndex = distanceTuples[i].index;
            }
        }

        // ��ʼ����
        var alignPos = new Vector2();
        // ˮƽ����
        var offset = judge.judgePosition.x - halfNote.path[closedJudgePointIndex].x;
        alignPos.x = halfNote.noteMoveInfo.p0.x + offset;
        // ��ֱ����
        distanceFromJudge = judge.judgePosition.y - halfNote.path[closedJudgePointIndex].y;
        // �����ӽ��ж��㣬����û����������û�취
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
            // ��˵�������켣����ˮ���ֻ��һ�������ƶ���������
            if (waterFacePointIndex == 0)
            {
                var maxOffset = halfNote.path.Min(pos => ConstData.WaterFaceHeight - pos.y);
                alignPos.y = halfNote.noteMoveInfo.p0.y + Mathf.Min(distanceFromJudge, maxOffset);
            }
            else
            {
                // >0����ƽ��
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

        // ʱ�����
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
        // λ������ٶ��룬��Ϊ�����·�����㷽��
        halfNote.SetInitPos(alignPos);
    }

    /// <summary>
    /// ���y�������ж���İ�note�Ƿ���Զ���
    /// ������Զ��룬˵������·������ˮ��
    /// </summary>
    /// <param name="halfNote"></param>
    /// <param name="judge"></param>
    /// <returns>�ɷ����</returns>
    private bool CheckCanAlignInWater(EditNoteInfo halfNote, JudgePair judge)
    {
        var distanceFromWaterFace = halfNote.path.Min(pos => Mathf.Abs(ConstData.WaterFaceHeight - pos.y));
        var distanceFromJudgePos = halfNote.path.Min(pos => Mathf.Abs(judge.judgePosition.y - pos.y));

        return distanceFromWaterFace > distanceFromJudgePos;
    }

    #endregion
}
