using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 画面渲染管理
/// </summary>
public class ImageRenderManager : MonoBehaviour
{
    public static ImageRenderManager Instance;

    /// <summary>主画板</summary>
    public RectTransform mainCanvas;

    /// <summary>note移动路径数据</summary>
    private List<SingleLinesInfo> noteMovePathLineInfos;
    /// <summary>小节线数据</summary>
    private List<SingleLinesInfo> measureLineInfos;
    /// <summary>判定点绘制数据（圆）</summary>
    private List<SingleLinesInfo> judgeNoteInfos;
    /// <summary>hold路径绘制数据</summary>
    private List<SingleLinesInfo> holdPathInfos;
    /// <summary>鼠标的坐标引导线</summary>
    private List<SingleLinesInfo> mouseCoordinateGuideLineInfos;
    /// <summary>正在编辑的判定的引导线</summary>
    private List<SingleLinesInfo> editingJudgeGuidLineInfos;

    /// <summary>小节线的文字</summary>
    private List<SingleTextInfo> measureTextInfos;
    /// <summary>鼠标的坐标引导线的文字</summary>
    private List<SingleTextInfo> mouseCoordinateGuidPosTextInfos;

    /// <summary>图片数据</summary>
    public List<SingleImageInfo> ImageInfos;
    private List<Texture2D> images;

    /// <summary>线段材质</summary>
    private Material lineMaterial;
    /// <summary>画板宽</summary>
    private float canvasWidth;
    /// <summary>画板高</summary>
    private float canvasHeight;
    /// <summary>GUI样式</summary>
    private GUIStyle guiStyle;

    #region Public Method

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        noteMovePathLineInfos = new List<SingleLinesInfo>();
        measureLineInfos = new List<SingleLinesInfo>();
        judgeNoteInfos = new List<SingleLinesInfo>();
        holdPathInfos = new List<SingleLinesInfo>();
        mouseCoordinateGuideLineInfos = new List<SingleLinesInfo>();
        editingJudgeGuidLineInfos = new List<SingleLinesInfo>();

        measureTextInfos = new List<SingleTextInfo>();
        mouseCoordinateGuidPosTextInfos = new List<SingleTextInfo>();

        ImageInfos = new List<SingleImageInfo>();
        images = new List<Texture2D>();
        InitMaterial();
        InitGUIStyle();
        LoadImages();
    }

    /// <summary>
    /// 图片加载
    /// </summary>
    public void LoadImages()
    {
        ConstData.ImageNames.ForEach(image => images.Add(Resources.Load<Texture2D>(image)));
    }

    public void OnUpdate()
    {
        canvasWidth = mainCanvas.rect.width;
        canvasHeight = mainCanvas.rect.height;
    }

    /// <summary>
    /// 屏幕坐标转换为画板坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>画板坐标</returns>
    public (float x, float y) TransToCanvasPoint(float x, float y)
    {
        return (x / Screen.width * canvasWidth, y / Screen.height * canvasHeight);
    }

    /// <summary>
    /// 屏幕坐标转换为画板坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>画板坐标</returns>
    public Vector2 TransToCanvasPoint(Vector2 pos)
    {
        var res = TransToCanvasPoint(pos.x, pos.y);
        return new Vector2(res.x, res.y);
    }

    public void AddMeasureTextInfo(float x, float y, string content)
    {
        measureTextInfos.Add(new SingleTextInfo(x / canvasWidth * Screen.width, (canvasHeight - y) / canvasHeight * Screen.height, content));
    }

    public void ClearMeasureTextInfos()
    {
        measureTextInfos.Clear();
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="index">序号</param>
    /// <param name="x">位置x</param>
    /// <param name="y">位置y</param>
    /// <param name="w">宽度</param>
    /// <param name="h">长度</param>
    public void AddImage(int index, float x, float y, float w, float h)
    {
        ImageInfos.Add(new SingleImageInfo(x / canvasWidth * Screen.width, y / canvasHeight * Screen.height, w / canvasWidth * Screen.width, h / canvasHeight * Screen.height, index));
    }

    public void AddMeasureLine(Color color, List<Vector2> points)
    {
        measureLineInfos.Add(CreateLinesInfo(color, points));
    }

    public void ClearMeasureLines()
    {
        measureLineInfos.Clear();
    }

    public void AddNoteMovePathLine(Color color, List<Vector2> path)
    {
        noteMovePathLineInfos.Add(CreateLinesInfo(color, path));
    }

    public void ClearNoteMovePathLines()
    {
        noteMovePathLineInfos.Clear();
    }

    /// <summary>
    /// 添加判定点的线（圆的边）
    /// </summary>
    /// <param name="color"></param>
    /// <param name="edges">边线</param>
    public void AddJudgeNoteInfoLine(Color color, List<Vector2> edges)
    {
        judgeNoteInfos.Add(CreateLinesInfo(color, edges));
    }

    public void ClearJudgeNoteInfos()
    {
        judgeNoteInfos.Clear();
    }

    /// <summary>
    /// 添加hold路径的线
    /// </summary>
    /// <param name="color"></param>
    /// <param name="path">路径</param>
    public void AddHoldPathInfoLine(Color color, List<Vector2> path)
    {
        holdPathInfos.Add(CreateLinesInfo(color, path));
    }

    public void ClearHoldPathInfos()
    {
        holdPathInfos.Clear();
    }

    public void AddMouseCoordinateLine(Color color, List<Vector2> points)
    {
        mouseCoordinateGuideLineInfos.Add(CreateLinesInfo(color, points));
    }

    public void ClearMousePositionCoordinateInfo()
    {
        mouseCoordinateGuideLineInfos.Clear();
    }

    public void AddEditingJudgeCoordinateLine(Color color, List<Vector2> points)
    {
        editingJudgeGuidLineInfos.Add(CreateLinesInfo(color, points));
    }

    public void ClearEditingJudgeCoordinateLine()
    {
        editingJudgeGuidLineInfos.Clear();
    }

    public void AddCoordinateTextInfo(float x, float y, string content)
    {
        mouseCoordinateGuidPosTextInfos.Add(new SingleTextInfo(x / canvasWidth * Screen.width, (canvasHeight - y) / canvasHeight * Screen.height, content));
    }

    public void ClearCoordinateTextInfo()
    {
        mouseCoordinateGuidPosTextInfos.Clear();
    }

    #endregion

    #region Private Method

    private SingleLinesInfo CreateLinesInfo(Color color, List<Vector2> path)
    {
        SingleLinesInfo linesInfo = new SingleLinesInfo(color);
        for (int i = 0; i < path.Count - 1; i++)
        {
            var start = new Vector2(path[i].x / canvasWidth, path[i].y / canvasHeight);
            var end = new Vector2(path[i + 1].x / canvasWidth, path[i + 1].y / canvasHeight);
            linesInfo.AddLine(start, end);
        }

        return linesInfo;
    }

    /// <summary>
    /// 以<paramref name="color"/>的颜色画线
    /// </summary>
    /// <param name="point">端点</param>
    /// <param name="color">颜色</param>
    private void DrawLine(LineSegment point, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(new Vector2(point.startPoint.x, point.startPoint.y));
        GL.Vertex(new Vector2(point.endPoint.x, point.endPoint.y));
        GL.End();
    }

    /// <summary>
    /// 初始化线段材质
    /// </summary>
    private void InitMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
    }

    /// <summary>
    /// 初始化GUI样式
    /// </summary>
    private void InitGUIStyle()
    {
        guiStyle = new GUIStyle
        {
            fontSize = 20,
            normal = {textColor = Color.white}
        };
    }

    private void DrawLines(SingleLinesInfo info)
    {
        info.lines.ForEach(line => DrawLine(line, info.lineColor));
    }

    private void DrawText(SingleTextInfo info)
    {
        GUI.Label(new Rect(info.x, info.y, 100, 20), info.text, guiStyle);
    }

    #endregion

    #region Unity

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Update()
    {
        OnUpdate();
    }

    void OnPostRender()
    {
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.LoadOrtho();
        GL.PushMatrix();
        lineMaterial.SetPass(0);
        
        judgeNoteInfos.ForEach(DrawLines);
        holdPathInfos.ForEach(DrawLines);
        noteMovePathLineInfos.ForEach(DrawLines);
        
        measureLineInfos.ForEach(DrawLines);

        mouseCoordinateGuideLineInfos.ForEach(DrawLines);
        editingJudgeGuidLineInfos.ForEach(DrawLines);
        GL.PopMatrix();
    }

    void OnGUI()
    {
        mouseCoordinateGuidPosTextInfos.ForEach(DrawText);
        measureTextInfos.ForEach(DrawText);
    }

    #endregion
}

public class SingleLinesInfo
{
    public Color lineColor;
    public List<LineSegment> lines;

    public SingleLinesInfo(Color c)
    {
        lineColor = c;
        lines = new List<LineSegment>();
    }

    public void AddLine((float, float) p1, (float, float) p2)
    {
        lines.Add(new LineSegment(p1, p2));
    }

    public void AddLine(Vector2 p1, Vector2 p2)
    {
        lines.Add(new LineSegment((p1.x, p1.y), (p2.x, p2.y)));
    }
}

public struct SingleTextInfo
{
    public float x;
    public float y;
    public string text;

    public SingleTextInfo(float inx, float iny, string t)
    {
        x = inx;
        y = iny;
        text = t;
    }
}

/// <summary>
/// 线段
/// </summary>
public struct LineSegment
{
    public (float x, float y) startPoint;
    public (float x, float y) endPoint;

    public LineSegment((float, float) s, (float, float) e)
    {
        startPoint = s;
        endPoint = e;
    }
}

public struct SingleImageInfo
{
    public float x;
    public float y;
    public float w;
    public float h;
    public int id;

    public SingleImageInfo(float inx, float iny, float inw, float inh, int tid)
    {
        x = inx;
        y = iny;
        w = inw;
        h = inh;
        id = tid;
    }
}