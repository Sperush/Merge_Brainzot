using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [Header("Cài đặt")]
    [SerializeField] private RawImage _img;
    [SerializeField] private float _xVelocity = 0.1f; // Tốc độ trôi ngang
    [SerializeField] private float _yVelocity = 0.1f; // Tốc độ trôi dọc

    void Awake()
    {
        // Tự động lấy component nếu chưa gán
        if (_img == null) _img = GetComponent<RawImage>();
    }

    void Update()
    {
        // Lấy hình chữ nhật UV hiện tại
        Rect currentRect = _img.uvRect;

        // Cộng thêm vị trí dựa trên thời gian và tốc độ
        currentRect.x += _xVelocity * Time.deltaTime;
        currentRect.y += _yVelocity * Time.deltaTime;

        // Gán ngược lại vào RawImage
        _img.uvRect = currentRect;
    }
}