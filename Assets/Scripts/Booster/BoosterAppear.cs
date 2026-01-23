using UnityEngine;
using DG.Tweening;

public class BoosterAppear : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float startOffsetY = -300f; // kéo từ dưới lên

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 targetPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        targetPos = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        Play();
    }

    public void Play()
    {
        // Set trạng thái ban đầu
        rect.anchoredPosition = targetPos + Vector2.up * startOffsetY;
        canvasGroup.alpha = 0f;

        // Kill tween cũ nếu có
        rect.DOKill();
        canvasGroup.DOKill();

        // Move + Fade song song
        rect.DOAnchorPos(targetPos, moveDuration)
            .SetEase(Ease.OutBack);

        canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.Linear);
    }
}
