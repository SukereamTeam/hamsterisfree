using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    [SerializeField]
    private Vector2 fixedRatio = Vector2Int.zero;

    private void Start()
    {
        SetResolution();
    }

    private void SetResolution()
    {
        var targetAspect = fixedRatio.y / fixedRatio.x;
        float currentAspect = (float)Screen.height / Screen.width;
        Camera mainCamera = this.GetComponent<Camera>();

        if (currentAspect > targetAspect)
        {
            // 화면이 더 세로로 길 때 (레터박스가 위아래로 추가됨)
            float scaleHeight = targetAspect / currentAspect;
            var orthographicSize = mainCamera.orthographicSize;
            var size = orthographicSize - (orthographicSize * scaleHeight);
            mainCamera.orthographicSize = orthographicSize + size;
        }
        else
        {
            // 화면이 더 가로로 길 때 (레터박스가 좌우로 추가됨)
            mainCamera.orthographicSize = mainCamera.orthographicSize;
        }
    }
}
