using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public bool isEnd;
    public void Init(Action dropCallback, bool isgift = false)
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        isGift = isgift;
        onDropBomb = dropCallback;
        dropped = false;
        hasEnteredBackground = false;
        isEnd = false;
        StartCoroutine(ResetVisuals());
    }
    IEnumerator ResetVisuals()
    {
        var anim = GetComponent<Animator>();
        var skins = GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteSkin>();
        if (anim != null) anim.enabled = false;
        foreach (var s in skins)
            if (s != null) s.enabled = false;
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (isGift)
        {
            foreach (var s in skins)
                if (s != null) s.enabled = true;
        }
        if (anim != null)
        {
            anim.enabled = true;
            //anim.Rebind();
            anim.Update(0f);
        }
        StartFly();
    }
    public IEnumerator ResetVisuals(Queue<BombPlane> p)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        var anim = GetComponent<Animator>();
        var skins = GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteSkin>();

        if (anim != null) anim.enabled = false;
        foreach (var s in skins)
            if (s != null) s.enabled = false;
        yield return null;
        yield return null;

        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(-0.2f, 0.18f, 10));
        if(!isGift) Destroy(gameObject);
        else gameObject.SetActive(false);
        //p.Enqueue(this);
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
        if (isEnd) return;
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
        if (hasEnteredBackground && (transform.position.x >= endPos.x || (!BattleManager.Instance.startPvP && !isGift)))
        {
            isEnd = true;
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
