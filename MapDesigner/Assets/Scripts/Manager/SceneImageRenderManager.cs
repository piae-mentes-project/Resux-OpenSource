using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneImageRenderManager
{
    private static SceneImageRenderManager instance;
    public static SceneImageRenderManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SceneImageRenderManager();
            }

            return instance;
        }
    }

    #region Properties

    /// <summary>�ֱ��ʹ���</summary>
    private ResolutionManager ResolutionManager => ResolutionManager.Instance;
    /// <summary>ͼ��UI����</summary>
    private ImageRenderManager ImageUIManager => ImageRenderManager.Instance;

    private List<InGameJudgeNoteInfo> showingHoldNotes;

    #endregion

    private SceneImageRenderManager()
    {
        showingHoldNotes = new List<InGameJudgeNoteInfo>();
    }

    #region Public Method

    /// <summary>
    /// ��������Ԥ����
    /// </summary>
    public void DrawPreviewLines()
    {
        showingHoldNotes.Clear();
        ImageUIManager.ClearNoteMovePathLines();
        ImageUIManager.ClearJudgeNoteInfos();
        ImageUIManager.ClearHoldPathInfos();
        foreach (InGameJudgeNoteInfo judgeNoteInfo in EditingData.GetValidJudgeNotes())
        {
            DrawSingleJudgePoint(judgeNoteInfo);

            var upNote = judgeNoteInfo.movingNotes.up;
            if (upNote != null)
            {
                DrawSingleHalfNoteMovePathPreview(Color.yellow, upNote.path);
            }

            var downNote = judgeNoteInfo.movingNotes.down;
            if (downNote != null)
            {
                DrawSingleHalfNoteMovePathPreview(Color.yellow, downNote.path);
            }

            DrawSingleHoldPath(judgeNoteInfo);
        }

        DrawEditingJudgeNoteGuideLine();
        DrawEditingHoldNote();
    }

    /// <summary>
    /// ��������Note·��Ԥ��
    /// </summary>
    /// <param name="noteMovePath">��note�˶�·��</param>
    public void DrawSingleHalfNoteMovePathPreview(Color color, Vector2[] noteMovePath)
    {
        if (noteMovePath.Length != 0)
        {
            var paintPath = new List<Vector2>();
            for (int i = 0; i < noteMovePath.Length; i++)
            {
                paintPath.Add(TransPosFromRealPos2PaintPos(noteMovePath[i]));
            }
            ImageUIManager.AddNoteMovePathLine(color, paintPath);
        }
    }

    /// <summary>
    /// ����ˢ���ж����Ԥ��
    /// </summary>
    public void RefreshJudgeNotePreview()
    {
        showingHoldNotes.Clear();
        ImageUIManager.ClearJudgeNoteInfos();
        foreach (InGameJudgeNoteInfo judgeNoteInfo in EditingData.GetValidJudgeNotes())
        {
            DrawSingleJudgePoint(judgeNoteInfo);
        }

        DrawEditingHoldNote();
    }

    /// <summary>
    /// ֻˢ��hold·��
    /// </summary>
    public void RefreshHoldJudgePathPreview()
    {
        ImageUIManager.ClearHoldPathInfos();
        foreach (InGameJudgeNoteInfo judgeNoteInfo in EditingData.GetValidJudgeNotes())
        {
            DrawSingleHoldPath(judgeNoteInfo);
        }
    }

    public void DrawEditingJudgeNoteGuideLine()
    {
        ImageUIManager.ClearEditingJudgeCoordinateLine();
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            return;
        }

        var judgePos = EditingData.CurrentEditingJudgeNote.judge.judgePosition;
        var gameAreaLeftDownPositionInMainPanel = EditingData.GameAreaLeftDownPositionInMainPanel;
        var gameAreaRightUpPositionInMainPanel = EditingData.GameAreaRightUpPositionInMainPanel;
        var paintJudgePos = TransPosFromRealPos2PaintPos(judgePos);
        var horizontalLine = new List<Vector2>()
        {
            new Vector2(gameAreaLeftDownPositionInMainPanel.x, paintJudgePos.y),
            new Vector2(gameAreaRightUpPositionInMainPanel.x, paintJudgePos.y)
        };
        var verticalLine = new List<Vector2>()
        {
            new Vector2(paintJudgePos.x, gameAreaLeftDownPositionInMainPanel.y),
            new Vector2(paintJudgePos.x, gameAreaRightUpPositionInMainPanel.y)
        };
        ImageUIManager.AddEditingJudgeCoordinateLine(Color.red, horizontalLine);
        ImageUIManager.AddEditingJudgeCoordinateLine(Color.red, verticalLine);
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void DrawCoordinateGuideLine()
    {
        ImageUIManager.ClearMousePositionCoordinateInfo();
        ImageUIManager.ClearCoordinateTextInfo();
        var mousePositionInMainPanel = EditingData.MousePositionInMainPanel;
        var gameAreaLeftDownPositionInMainPanel = EditingData.GameAreaLeftDownPositionInMainPanel;
        var gameAreaRightUpPositionInMainPanel = EditingData.GameAreaRightUpPositionInMainPanel;
        var mousePositionInGamePosition = EditingData.MousePositionInGameAreaWithScale;
        var horizontalLine = new List<Vector2>()
        {
            new Vector2(gameAreaLeftDownPositionInMainPanel.x, mousePositionInMainPanel.y),
            new Vector2(gameAreaRightUpPositionInMainPanel.x, mousePositionInMainPanel.y)
        };
        var verticalLine = new List<Vector2>()
        {
            new Vector2(mousePositionInMainPanel.x, gameAreaLeftDownPositionInMainPanel.y),
            new Vector2(mousePositionInMainPanel.x, gameAreaRightUpPositionInMainPanel.y)
        };
        ImageUIManager.AddMouseCoordinateLine(Color.white, horizontalLine);
        ImageUIManager.AddMouseCoordinateLine(Color.white, verticalLine);
        ImageUIManager.AddCoordinateTextInfo(mousePositionInMainPanel.x > Screen.width - 200 ? mousePositionInMainPanel.x - 200 : mousePositionInMainPanel.x + 20, mousePositionInMainPanel.y, $"({mousePositionInGamePosition.x: 0.00}, {mousePositionInGamePosition.y: 0.00})");
    }

    public void DrawEditingHoldNote()
    {
        var holdNote = EditingData.CurrentEditingJudgeNote;
        if (holdNote == null || holdNote.judgeType != JudgeType.Hold)
        {
            return;
        }

        if (showingHoldNotes.Contains(holdNote))
        {
            return;
        }

        // ����hold���ж��༭״̬�²Ŷ�����ʾ�ж����Ը���
        if (EditingData.CurrentEditStep == EditPanelView.EditStep.HoldJudge ||
            EditingData.CurrentEditStep == EditPanelView.EditStep.HoldPath)
        {
            DrawSingleJudgePoint(holdNote);
            DrawSingleHoldPath(holdNote);
        }
    }

    /// <summary>
    /// ������е�Ԥ��������Ⱦ
    /// </summary>
    public void ClearAllRender()
    {
        ImageUIManager.ClearNoteMovePathLines();
        ImageUIManager.ClearJudgeNoteInfos();
        ImageUIManager.ClearHoldPathInfos();
    }

    #endregion

    #region Private Method

    private void DrawSingleJudgePoint(InGameJudgeNoteInfo judgeNoteInfo)
    {
        if (judgeNoteInfo.judgeType == JudgeType.Hold)
        {
            // ��Ӷ���ʾ�е�hold�ļ�¼
            showingHoldNotes.Add(judgeNoteInfo);
        }

        foreach (var judge in judgeNoteInfo.judges)
        {
            ImageUIManager.AddJudgeNoteInfoLine(ConstData.JudgeNoteColor[judgeNoteInfo.judgeType],
                GetPointsByCenterRadius(TransPosFromRealPos2PaintPos(judge.judgePosition), 10));
        }
    }

    private void DrawSingleHoldPath(InGameJudgeNoteInfo judgeNoteInfo)
    {
        if (judgeNoteInfo.judgeType != JudgeType.Hold || judgeNoteInfo.movingNotes.up == null)
        {
            return;
        }

        var paintPath = new List<Vector2>();
        foreach (var point in judgeNoteInfo.movingNotes.up.holdPath)
        {
            paintPath.Add(TransPosFromRealPos2PaintPos(point));
        }

        var holdColor = ConstData.JudgeNoteColor[JudgeType.Hold];
        // var color = judgeNoteInfo == EditingData.CurrentEditingJudgeNote ? holdColor + new Color(0.15f, 0.15f, 0) : holdColor;
        ImageUIManager.AddHoldPathInfoLine(holdColor, paintPath);
    }

    private List<Vector2> GetPointsByCenterRadius(Vector2 center, float radius)
    {
        var edges = new List<Vector2>();
        // 32����������һ�µ��ˣ��ջ���Ҫ��β����������33
        var pointCount = 33;
        for (int i = 0; i < pointCount; i++)
        {
            var point = center + new Vector2(radius * Mathf.Cos(ConstData.PI2 * i / pointCount),
                radius * Mathf.Sin(ConstData.PI2 * i / pointCount));
            edges.Add(point);
        }

        return edges;
    }

    private Vector2 TransPosFromRealPos2PaintPos(Vector2 realPos)
    {
        var gameWindow1080Pos = Tools.ScaleToGameWindowPosition(realPos);
        return ImageUIManager.TransToCanvasPoint(ResolutionManager.TransToScenePosition(gameWindow1080Pos));
    }

    #endregion
}
