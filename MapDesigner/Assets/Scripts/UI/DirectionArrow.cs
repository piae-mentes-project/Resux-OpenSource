using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    #region properties

    [SerializeField] RectTransform startTransform;
    [SerializeField] RectTransform areaTransform;

    private RectTransform rectTransform;
    private RectTransform selfTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            return rectTransform;
        }
    }

    #endregion

    void Update()
    {
        
    }

    #region Public Method

    public void RefreshDirection(Vector2 speed)
    {
        var xRange = ConstData.HorizontalSpeedRange;
        var yRange = ConstData.VerticalSpeedRange;
        var size = areaTransform.sizeDelta;
        float x, y;
        // 因为速度范围是从负到正
        if (speed.x > 0)
        {
            x = speed.x / (xRange.max - 0) * size.x / 2;
        }
        else
        {
            x = speed.x / (-xRange.min) * size.x / 2;
        }

        if (speed.y > 0)
        {
            y = speed.y / (yRange.max - 0) * size.y / 2;
        }
        else
        {
            y = speed.y / (-yRange.min) * size.y / 2;
        }

        var angleZ = Vector2.SignedAngle(Vector2.right, speed);
        selfTransform.eulerAngles = new Vector3(0, 0, angleZ);

        selfTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new Vector2(x, y).magnitude);
    }

    #endregion
}
