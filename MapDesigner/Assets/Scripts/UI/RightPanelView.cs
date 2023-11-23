using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightPanelView : BaseView
{
    #region properties

    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        MusicPlayManager.Instance.AddMusicPlayingListener(LoadMeasureLine);
        plusButton.onClick.AddListener(OnPlusButton);
        minusButton.onClick.AddListener(OnMinusButton);

        EditingData.AddPartialCountForEditChangeListener(musicTime => LoadMeasureLine(musicTime));
    }

    public override void ResetView()
    {
        LoadMeasureLine(MusicPlayManager.Instance.MusicTime);
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 加载小节线
    /// </summary>
    /// <param name="musicTime">音乐时间（s）</param>
    private void LoadMeasureLine(float musicTime)
    {
        // 因为监听音乐的时间进度的时候Editing中的数据还未更新，因此必须用参数
        var currentTime = (int)(musicTime * 1000 + 0.5f);
        // var currentTime = EditingData.CurrentTimeWithoutDelay;
        var ImageUIManager = ImageRenderManager.Instance;
        ImageUIManager.ClearMeasureLines();
        ImageUIManager.ClearMeasureTextInfos();
        var (width, height) = EditingData.MainCanvasSize;
        var nowBeat = (int)Tools.TransTimeToBeat(currentTime, MapDesignerSettings.BpmList, out var bpm);
        var previewMeasureCountRadius = GlobalSettings.MaxPreviewMeasureCount / 2;
        for (int i = 0; i < EditingData.PartialCountForEdit; ++i)
        {
            var color = ConstData.BeatColors[EditingData.PartialIndexForEdit][i];
            var points = new List<Vector2>();
            for (int j = nowBeat - previewMeasureCountRadius; j < nowBeat + previewMeasureCountRadius; ++j)
            {
                points.Clear();
                float nowCalcPos = Tools.TransBeatToTime(new Beat(j, i, EditingData.PartialCountForEdit), MapDesignerSettings.BpmList);
                float tPos = (nowCalcPos - currentTime) * EditingData.BpmScale + (height - 150) / 2;
                if (tPos > 0 && tPos < height - 150)
                {
                    points.Add(new Vector2(width - 100, tPos));
                    points.Add(new Vector2(width, tPos));
                    ImageUIManager.AddMeasureLine(color, points);
                }
                else if (j > nowBeat + 10)
                {
                    break;
                }
            }
        }
        for (int i = nowBeat - previewMeasureCountRadius; i < nowBeat + previewMeasureCountRadius; ++i)
        {
            float nowCalcPos = Tools.TransBeatToTime(new Beat(i, 0, EditingData.PartialCountForEdit), MapDesignerSettings.BpmList);
            float tPos = (nowCalcPos - currentTime) * EditingData.BpmScale + (height - 150) / 2;
            if (tPos > 0 && tPos < height - 150)
            {
                ImageUIManager.AddMeasureTextInfo(width - 100, tPos, i.ToString());
            }
            else if (i > nowBeat + 10)
            {
                break;
            }
        }

        var posY = (height - 150) / 2;
        var redLinePoints = new List<Vector2>()
        {
            new Vector2(width - 100, posY),
            new Vector2(width, posY)
        };
        ImageUIManager.AddMeasureLine(Color.red, redLinePoints);
    }

    private void OnPlusButton()
    {
        var scale = EditingData.BpmScale + 0.2f;
        EditingData.BpmScale = Mathf.Clamp(scale, 0.4f, 1.6f);
        LoadMeasureLine(MusicPlayManager.Instance.MusicTime);
    }

    private void OnMinusButton()
    {
        var scale = EditingData.BpmScale - 0.2f;
        EditingData.BpmScale = Mathf.Clamp(scale, 0.4f, 1.6f);
        LoadMeasureLine(MusicPlayManager.Instance.MusicTime);
    }

    #endregion
}
