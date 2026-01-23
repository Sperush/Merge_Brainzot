using UnityEngine;
using System;
using System.Collections;

public class BombPlane : MonoBehaviour
{
    public float speed;
    public bool isGift;
    private Action onDropBomb;
    private bool dropped;
    private float targetDropX = 0f;

    private Vector3 endPos;
    private Vector3 dir;
    public GameObject[] buttonGift;
    private bool hasEnteredBackground = false;
    public void Init(Action dropCallback, bool isgift = false)
    {
        gameObject.SetActive(true);
        isGift = isgift;
        onDropBomb = dropCallback;
        dropped = false;
        hasEnteredBackground = false;
        StartFly(UnityEngine.Random.value < 0.5);
    }
    bool IsInsideBackground()
    {
        return LevelBgrManager.Instance.bgr.bounds.Contains(transform.position);
    }
    bool IsOutOfBackground()
    {
        Bounds bgBounds = LevelBgrManager.Instance.bgr.bounds;
        return !bgBounds.Contains(transform.position);
    }

    void StartFly(bool isLeftToRight)
    {
        float y = Camera.main.ViewportToWorldPoint(new Vector3(0, UnityEngine.Random.Range(0.75f, 0.85f), 10)).y;
        Vector3 start, end;

        if (isLeftToRight)
        {
            start = Camera.main.ViewportToWorldPoint(new Vector3(-0.2f, 0.8f, 10));
            end = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.8f, 10));
            transform.localScale = Vector3.one;
        }
        else
        {
            start = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.8f, 10));
            end = Camera.main.ViewportToWorldPoint(new Vector3(-0.2f, 0.8f, 10));
            transform.localScale = new Vector3(-1, 1, 1); // lật sprite
        }
        start.y = end.y = y;
        transform.position = start;
        endPos = end;
        dir = (end - start).normalized;
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        if (!hasEnteredBackground && IsInsideBackground())
        {
            hasEnteredBackground = true;
        }
        // ===== THẢ BOM TẠI X = 0 =====
        if (!dropped && HasReachedDropX() && !isGift)
        {
            dropped = true;
            onDropBomb?.Invoke();
        }

        // Kết thúc đường bay
        if (hasEnteredBackground  && (IsOutOfBackground() || (!BattleManager.Instance.startPvP && !isGift)))
        {
            if (isGift)
            {
                buttonGift[transform.localScale.x == 1 ? 1 : 0].SetActive(true);
            }
            BombPlanePool.Instance.Release(this);
        }
    }
    bool HasReachedDropX()
    {
        // Bay từ trái sang phải
        if (dir.x > 0)
            return transform.position.x >= targetDropX;

        // Bay từ phải sang trái
        return transform.position.x <= targetDropX;
    }
}
