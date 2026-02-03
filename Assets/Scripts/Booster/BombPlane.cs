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
    public GameObject buttonGift;
    private bool hasEnteredBackground = false;
    public void Init(Action dropCallback, bool isgift = false)
    {
        gameObject.SetActive(true);

        isGift = isgift;
        onDropBomb = dropCallback;
        dropped = false;
        hasEnteredBackground = false;

        StartCoroutine(ResetVisuals()); // tên rõ nghĩa hơn
        StartFly();
    }
    IEnumerator ResetVisuals()
    {
        var anim = GetComponent<Animator>();
        var skins = GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteSkin>();

        // stop jobs
        if (anim != null) anim.enabled = false;
        foreach (var s in skins)
            if (s != null) s.enabled = false;

        yield return null;
        yield return null; // an toàn với job

        // resume
        foreach (var s in skins)
            if (s != null) s.enabled = true;

        if (anim != null)
        {
            anim.enabled = true;
            anim.Rebind();
            anim.Update(0f);
        }
    }

    public static bool IsInsideBackground(Vector3 pos)
    {
        return LevelBgrManager.Instance.bgr.bounds.Contains(pos);
    }
    public static bool IsOutOfBackground(Vector3 pos)
    {
        Bounds bgBounds = LevelBgrManager.Instance.bgr.bounds;
        return !bgBounds.Contains(pos);
    }

    void StartFly()
    {
        float y = Camera.main.ViewportToWorldPoint(new Vector3(0, UnityEngine.Random.Range(0.3f, 0.5f), 10)).y;
        Vector3 start, end;
        start = Camera.main.ViewportToWorldPoint(new Vector3(-0.2f, 0.18f, 10));
        end = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.18f, 10));
        start.y = end.y = y;
        transform.position = start;
        endPos = end;
        dir = (end - start).normalized;
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        if (!hasEnteredBackground && IsInsideBackground(transform.position))
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
        if (hasEnteredBackground  && (IsOutOfBackground(transform.position) || (!BattleManager.Instance.startPvP && !isGift)))
        {
            if (isGift)
            {
                buttonGift.SetActive(true);
            }
            BombPlanePool.Instance.Release(this);
        }
    }
    bool HasReachedDropX()
    {
        return transform.position.x >= targetDropX;
    }
}
