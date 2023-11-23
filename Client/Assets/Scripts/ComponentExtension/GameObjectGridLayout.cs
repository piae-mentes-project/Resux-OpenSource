using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>轴向</summary>
public enum Axis
{
    Horizontal = 0,
    Vertical = 1
}

public enum Corner
{
    UpperLeft = 0,
    UpperRight = 1,
    LowerLeft = 2,
    LowerRight = 3
}

public class GameObjectGridLayout : MonoBehaviour
{
    #region properties

    [SerializeField]
    private Vector2Int cellSize = new Vector2Int(20, 20);
    public Vector2Int CellSize => cellSize;
    [SerializeField]
    private Vector2Int gridSize = new Vector2Int(20, 20);
    public Vector2Int GridSize => gridSize;
    /// <summary>水平方向的格子数量</summary>
    public int HorizontalCount
    {
        get => GridSize.x;
        set
        {
            if (value > 0)
            {
                gridSize.x = value;
            }
        }
    }
    /// <summary>竖直方向的格子数量</summary>
    public int VerticalCount
    {
        get => GridSize.y;
        set
        {
            if (value > 0)
            {
                gridSize.y = value;
            }
        }
    }
    [SerializeField]
    private Axis direction;
    [SerializeField]
    private Corner startCorner;
    
    /// <summary>下一个空位</summary>
    private Vector2Int nextPos = new Vector2Int(0, 0);
    private List<GameObject> cellObjects = new List<GameObject>();

    #endregion

    #region unity

    private void Start()
    {
        
    }

    #endregion

    #region Public Method

    /// <summary>
    /// 添加单元格
    /// </summary>
    /// <param name="cellObj">单元格物体</param>
    public void AddCell(GameObject cellObj)
    {
        cellObjects.Add(cellObj);
        cellObj.transform.parent = transform;
        cellObj.transform.localPosition = GetNextPosition();
        RefreshNextPos();
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 刷新下一个空位（默认当前空位有单元格了）
    /// </summary>
    private void RefreshNextPos()
    {
        switch (direction)
        {
            case Axis.Horizontal:
                if (GridSize.x <= nextPos.x + 1)
                {
                    nextPos.x = 0;
                    nextPos.y++;
                }
                else
                {
                    nextPos.x++;
                }
                break;
            case Axis.Vertical:
                if (GridSize.y <= nextPos.y + 1)
                {
                    nextPos.y = 0;
                    nextPos.x++;
                }
                else
                {
                    nextPos.y++;
                }
                break;
        }
    }

    private Vector2 GetNextPosition()
    {
        Vector2 pos = new Vector2();

        switch (startCorner)
        {
            case Corner.UpperLeft:
                pos.x = CellSize.x * nextPos.x + CellSize.x / 2;
                pos.y = CellSize.y * (GridSize.y - nextPos.y - 1) + CellSize.y / 2;
                break;
            case Corner.UpperRight:
                // Upper
                pos.x = CellSize.x * (GridSize.x - nextPos.x - 1) + CellSize.x / 2;
                // Right
                pos.y = CellSize.y * (GridSize.y - nextPos.y - 1) + CellSize.y / 2;
                break;
            case Corner.LowerLeft:
                pos.x = CellSize.x * nextPos.x + CellSize.x / 2;
                pos.y = CellSize.y * nextPos.y + CellSize.y / 2;
                break;
            case Corner.LowerRight:
                pos.x = CellSize.x * (GridSize.x - nextPos.x - 1) + CellSize.x / 2;
                pos.y = CellSize.y * nextPos.y + CellSize.y / 2;
                break;
        }

        return pos;
    }

    #endregion
}
