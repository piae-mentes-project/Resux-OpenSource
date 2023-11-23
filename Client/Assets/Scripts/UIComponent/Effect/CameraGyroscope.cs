using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相机的陀螺仪脚本
/// </summary>
[ExecuteInEditMode]
public class CameraGyroscope : MonoBehaviour
{
    #region

    [SerializeField] [Tooltip("水平旋转范围（欧拉角偏移）")] private Vector2 xRotateRange;
    [SerializeField] [Tooltip("竖直旋转范围（欧拉角偏移）")] private Vector2 yRotateRange;
    [SerializeField] [Tooltip("相机弹回/归位动画位置曲线")] private AnimationCurve positionCurve;
    [SerializeField] [Tooltip("旋转阈值")] [Range(0.01f, 0.5f)] private float threshold = 0.1f;
    [SerializeField] [Tooltip("弹回用时（s）")] [Range(0.1f, 2f)] private float returnTime = 0.5f;
    [SerializeField] [Tooltip("弹回阈值（s）")] [Range(0.1f, 2f)] private float returnThreshold = 0.5f;

    [SerializeField] [Tooltip("缩放大小")] [Range(0.1f, 2)] private float scale = 2;

    [SerializeField] private Vector3 rawEulerAngles;
    [Space]
    [SerializeField] private RectTransform[] uiList;
    private List<Vector3> uiRawAngles;
    private List<Vector3> uiRawPositions;
    [SerializeField] private Vector3 uiMaxLocalAgnle;
    [SerializeField] private Vector3 uiMinLocalAgnle;
    // [SerializeField] private Vector3 uiMaxPosition;
    // [SerializeField] private Vector3 uiMinPosition;

    private Vector3 lastEulerAngles;
    private float time;
    private float totalDeltaTime;

    private bool isEnable;
    private Action onCameraReturn;

    #endregion

    private void OnEnable()
    {
        isEnable = Input.gyro.enabled = true;
    }

    void Start()
    {
        Input.gyro.enabled = true;
        // 陀螺仪数据的更新间隔
        Input.gyro.updateInterval = 0.02f;
        lastEulerAngles = rawEulerAngles = transform.localEulerAngles;
        time = returnTime;

        uiRawAngles = new List<Vector3>(uiList.Length);
        uiRawPositions = new List<Vector3>(uiList.Length);
        for (int i = 0; i < uiList.Length; i++)
        {
            var ui = uiList[i];
            uiRawAngles.Add(ui.localEulerAngles);
            uiRawPositions.Add(ui.position);
        }
    }

    private void OnDisable()
    {
        Input.gyro.enabled = false;
    }

    void Update()
    {
        // from：https://blog.csdn.net/weixin_43665612/article/details/115330643
        // 陀螺仪欧拉角轴说明：
        // z为向上的轴，x为向右的轴，y为向前的轴。
        // 初步估计，手机本身的轴如下：z向上，x向右，y为前，假设是一个右手坐标系
        // world轴的基准为面朝北，左为西。
        // z：手机顶部正对正西为0。顺时针旋转，范围为0-360
        // y：平放为0，右侧倾斜后范围为0-360
        // x：竖起手机屏幕靠近身体为0，手机右侧倾斜选90度角度变化为0->90，继续旋转90度为90->0，继续旋转90度为360->270，继续旋转90度为270->360
        // x转换成平放的话：值为90，向旋转90度到立起为0，反向立起为360-270

        // 欧拉角不好用，不用了，改用加速度

        var rotateSpeed = Input.gyro.rotationRateUnbiased;
        var angles = new Vector3(-rotateSpeed.x, -rotateSpeed.y, 0);
        if (isEnable && angles.magnitude > threshold)
        {
            angles *= scale;
            var nextAngle = transform.localEulerAngles + angles;
            nextAngle.x = Mathf.Clamp(nextAngle.x, rawEulerAngles.x + xRotateRange.x, rawEulerAngles.x + xRotateRange.y);
            nextAngle.y = Mathf.Clamp(nextAngle.y, rawEulerAngles.y + yRotateRange.x, rawEulerAngles.y + yRotateRange.y);
            lastEulerAngles = transform.localEulerAngles = nextAngle;
            time = 0;
            totalDeltaTime = 0;
        }
        else
        {
            var deltaTime = Time.deltaTime;
            totalDeltaTime += deltaTime;
            // 需要回弹
            if (totalDeltaTime > returnThreshold)
            {
                if (time < returnTime)
                {
                    time += deltaTime;
                    transform.localEulerAngles = Vector3.Lerp(lastEulerAngles, rawEulerAngles, positionCurve.Evaluate(time / returnTime));
                }
                else
                {
                    transform.localEulerAngles = rawEulerAngles;
                    onCameraReturn?.Invoke();
                    onCameraReturn = null;
                }
            }
        }

#if !UNITY_EDITOR
        var offsetAngle = GetOffsetLocalAngle() * 5;
        var positionOffset = offsetAngle * 10;
        for (int i = 0; i < uiList.Length; i++)
        {
            var ui = uiList[i];
            offsetAngle.x = Mathf.Clamp(offsetAngle.x, uiMinLocalAgnle.x, uiMaxLocalAgnle.x);
            offsetAngle.y = Mathf.Clamp(offsetAngle.y, uiMinLocalAgnle.y, uiMaxLocalAgnle.y);
            offsetAngle.z = Mathf.Clamp(offsetAngle.z, uiMinLocalAgnle.z, uiMaxLocalAgnle.z);
            ui.localEulerAngles = uiRawAngles[i] + offsetAngle;
            // x上负下正，y左负右正
            ui.position = uiRawPositions[i] + new Vector3(-positionOffset.y, positionOffset.x, 0);
        }
#endif
    }

    #region Public Method

    public Vector3 GetOffsetLocalAngle()
    {
        return transform.localEulerAngles - rawEulerAngles;
    }

    public void SetOffsetLocalAngle(Vector3 offsetAngle)
    {
        transform.localEulerAngles = rawEulerAngles + offsetAngle;
    }

    public void PauseWithReturnCall(Action callback)
    {
        isEnable = false;
        onCameraReturn = callback;
    }

    #endregion

    #region Private Method

    // private Vector3 ConvertGyroAngleToCameraAngle(Vector3 eluerAngles)
    // {
    //     // 需要把角度变化限制在lastStableEulerAngles为基础的一定范围内
    //     return transform.eulerAngles + new Vector3(-eluerAngles.x, -eluerAngles.y, eluerAngles.z) * 0.3f;
    // }

    #endregion
}
