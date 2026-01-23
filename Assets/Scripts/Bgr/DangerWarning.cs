using UnityEngine;
using DG.Tweening;
using System.Collections;

public enum TypeDanger
{
    Normal,
    Hard,
    VeryHard
}

public class DangerWarning : MonoBehaviour
{
    [Header("Settings")]
    public float moveDuration = 1f;    // Thời gian để di chuyển đến đích (trước là speed)
    public float scaleDuration = 0.5f; // Thời gian phóng to/thu nhỏ
    public float rotateDuration = 0.5f;// Thời gian xoay
    public float scaleSize = 1.2f;     // Kích thước khi to lên
    public float stayDelay = 2f;       // Thời gian dừng lại ở giữa màn hình trước khi thu về

    [Header("References")]
    public SpriteRenderer img;
    public GameObject dark;
    // Lưu trạng thái mặc định
    private Vector3 defaultScale;
    private Vector3 defaultPos;
    private Vector3 defaultRotation;
    private int defaultSortingOrder;
    public Sprite[] sprites;
    // Biến lưu Sequence để quản lý
    private DG.Tweening.Sequence warningSequence;

    public static DangerWarning Instance;

    public void Awake()
    {
        Instance = this;
        // Lưu lại vị trí ban đầu
        defaultPos = transform.position;
        defaultScale = transform.localScale;
        defaultRotation = transform.localRotation.eulerAngles;
        defaultSortingOrder = img.sortingOrder;
    }
    public void Show(TypeDanger type)
    {
        StartCoroutine(RunWarningProcess(type));
    }
    public IEnumerator RunWarningProcess(TypeDanger type)
    {
        //yield return new WaitForSeconds(stayDelay);
        yield return new WaitUntil(() => PanelManager.Instance.isOpenPanel == false);
        Color cl = img.color;
        // 1. Xử lý màu sắc
        if (type == TypeDanger.Normal)
        {
            cl.a = 0f;
            img.color = cl;
            // Nếu là Normal thì có thể không cần hiện warning, hoặc return luôn
            yield break;
        }
        else if (type == TypeDanger.Hard) img.sprite = sprites[0];
        else if (type == TypeDanger.VeryHard) img.sprite = sprites[1];
        cl.a = 1f;
        img.color = cl;
        // 2. Reset trạng thái cũ nếu đang chạy dở
        if (warningSequence != null) warningSequence.Kill(); // Hủy sequence cũ

        // Đưa vật thể về vị trí xuất phát ngay lập tức để chuẩn bị chạy
        transform.position = defaultPos;
        transform.localScale = defaultScale;
        transform.eulerAngles = defaultRotation;

        // Nếu có sequence cũ đang chạy thì kill ngay để tránh lỗi chồng chéo
        if (warningSequence != null) warningSequence.Kill();
        // 3. Tạo Sequence (Chuỗi hành động)
        warningSequence = DOTween.Sequence();
        warningSequence.AppendInterval(stayDelay);
        warningSequence.AppendCallback(() => {
            img.sortingOrder = 105;
            dark.SetActive(true);
        });
        // --- GIAI ĐOẠN 1: Dần về đích (Xuất hiện) ---
        // Dùng Join để các hành động diễn ra cùng lúc
        warningSequence.Append(transform.DOMove(Vector3.zero, moveDuration).SetEase(Ease.Linear));
        warningSequence.Join(transform.DOScale(defaultScale * scaleSize, scaleDuration).SetEase(Ease.Linear));
        warningSequence.Join(transform.DORotate(Vector3.zero, rotateDuration).SetEase(Ease.Linear));
        
        // --- GIAI ĐOẠN 2: Chờ (Delay) ---
        warningSequence.AppendInterval(stayDelay);
        warningSequence.AppendCallback(() => {
            img.sortingOrder = defaultSortingOrder;
            dark.SetActive(false);
        });
        // --- GIAI ĐOẠN 3: Thu dần về mặc định (Biến mất) ---
        warningSequence.Append(transform.DOMove(defaultPos, moveDuration).SetEase(Ease.Linear));
        warningSequence.Join(transform.DOScale(defaultScale, scaleDuration).SetEase(Ease.Linear));
        warningSequence.Join(transform.DORotate(defaultRotation, rotateDuration).SetEase(Ease.Linear));

        // (Tùy chọn) Gọi hàm gì đó khi hoàn tất toàn bộ quá trình
        warningSequence.OnComplete(() => {
            Debug.Log("Đã thu về xong!");
            //img.sortingOrder = 10;
            //dark.SetActive(false);
        });
    }
}