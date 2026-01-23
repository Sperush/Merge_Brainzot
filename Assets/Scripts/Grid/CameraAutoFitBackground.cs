using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoFitBackground : MonoBehaviour
{
    public SpriteRenderer background; // kéo BG vào đây
    public float padding = 0.2f;       // lề an toàn (tuỳ chọn)

    void Start()
    {
        FitCamera();
    }

    void FitCamera()
    {
        if (background == null)
        {
            Debug.LogError("CameraAutoFitBackground: Missing background reference!");
            return;
        }

        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;

        // Kích thước background theo world unit
        float bgWidth = background.bounds.size.x;
        float bgHeight = background.bounds.size.y;

        // Tỉ lệ màn hình
        float screenRatio = (float)Screen.width / Screen.height;
        float bgRatio = bgWidth / bgHeight;

        if (screenRatio >= bgRatio)
        {
            // Màn hình rộng hơn BG → fit theo chiều cao
            cam.orthographicSize = (bgHeight / 2f) + padding;
        }
        else
        {
            // Màn hình hẹp hơn BG → fit theo chiều rộng
            float sizeByWidth = (bgWidth / screenRatio) / 2f;
            cam.orthographicSize = sizeByWidth + padding;
        }

        // Center camera theo background
        cam.transform.position = new Vector3(
            background.bounds.center.x,
            background.bounds.center.y,
            cam.transform.position.z
        );
    }
}
