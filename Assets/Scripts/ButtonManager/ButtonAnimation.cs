using UnityEngine;
using UnityEngine.EventSystems; // Cần thiết để bắt sự kiện nhấn giữ
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float scaleSize = 1.2f;    // Kích thước khi to lên
    public float duration = 0.1f;    // Tốc độ phóng to/thu nhỏ
    public Ease easeType = Ease.OutQuad;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale == Vector3.zero ? Vector3.one: transform.localScale;
    }

    // Khi nhấn chuột xuống (hoặc chạm tay vào màn hình)
    public void OnPointerDown(PointerEventData eventData)
    {
        // Xóa các Tween cũ để tránh xung đột
        //transform.DOKill();
        DOTween.Kill("buttondown");
        // Phóng to lên
        transform.DOScale(originalScale * scaleSize, duration).SetEase(easeType).SetUpdate(true).SetId("buttondown");
    }

    // Khi nhả chuột ra
    public void OnPointerUp(PointerEventData eventData)
    {
        //transform.DOKill();
        DOTween.Kill("buttonup");
        // Thu nhỏ về trạng thái ban đầu
        transform.DOScale(originalScale, duration).SetEase(easeType).SetUpdate(true).SetId("buttonup");
    }
}
