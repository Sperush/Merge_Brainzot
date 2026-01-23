using DG.Tweening;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    public Transform fill;
    private float maxWidth = 1f;
    private float currentRatio = 1f;

    public void SetHP(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        float targetWidth = maxWidth * ratio;
        float targetPosX = -(maxWidth - targetWidth) / 2f;
        fill.DOKill();
        float duration = ratio < currentRatio ? 0.15f : 0.3f;
        fill.DOScaleX(targetWidth, duration).SetEase(Ease.OutCubic).SetUpdate(true);
        fill.DOLocalMoveX(targetPosX, duration).SetEase(Ease.OutCubic).SetUpdate(true);
        currentRatio = ratio;
    }
}
