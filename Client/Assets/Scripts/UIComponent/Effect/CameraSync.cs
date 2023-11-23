using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 用于保持相机与主相机同步
public class CameraSync : MonoBehaviour
{
    [SerializeField]
    public Camera MainCamera;
    private Camera selfCamera;
    // Start is called before the first frame update
    void Start()
    {
        selfCamera = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var mainPos = MainCamera.transform.position;
        var selfPos = transform.position;
        var pos = new Vector3(mainPos.x, mainPos.y, selfPos.z);
        transform.position = pos;
        selfCamera.orthographicSize = MainCamera.orthographicSize;
    }
}
