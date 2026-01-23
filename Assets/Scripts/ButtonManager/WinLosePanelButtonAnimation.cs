using DG.Tweening;
using UnityEngine;

public class WinLosePanelButtonAnimation : MonoBehaviour
{
    public GameObject noThanksButton; // Kéo Button vào đây trong Inspector
    public GameObject adsButton;
    public float delayTime = 5f;
    public float animDuration = 0.5f;

    void Start()
    {
        // Phóng to lên 1.2 lần trong 0.8 giây
        adsButton.transform.DOScale(1.2f, 0.8f)
        .SetEase(Ease.InOutSine) // Chuyển động mượt mà ở hai đầu
        .SetLoops(-1, LoopType.Yoyo).SetUpdate(true); // -1 là lặp vô tận, Yoyo là to ra rồi nhỏ lại
    }

    void OnEnable()
    {
        // 1. Thiết lập trạng thái ban đầu cho Button
        noThanksButton.SetActive(false);
        noThanksButton.transform.localScale = Vector3.zero;

        // 2. Đợi 5 giây rồi thực hiện
        // Dùng DOVirtual.DelayedCall để kích hoạt Active trước khi Tween
        DOVirtual.DelayedCall(delayTime, () => {
            noThanksButton.SetActive(true);
            noThanksButton.transform.DOScale(Vector3.one, animDuration).SetUpdate(true);
        }).SetLink(gameObject).SetUpdate(true); // Tự động xóa nếu Panel bị ẩn trước khi tới 5s
    }


}
