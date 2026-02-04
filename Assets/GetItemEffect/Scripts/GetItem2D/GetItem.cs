using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace GIE
{

    public class GetItem : MonoBehaviour
    {
        public Transform mTrans;
        public bool mIsInUse = false;
        System.Action mArriveAction;
        Transform mToWhere;
        public float moveSpeed = 2f;
        public Vector3 oldscale;
        public float PlayEffect(Vector3 from_where, Transform to_where, System.Action item_arrive_action)
        {
            mIsInUse = true;
            gameObject.SetActive(true);
            mTrans.position = from_where;
            mToWhere = to_where;
            mArriveAction = item_arrive_action;
            return Explosion();
        }

        float Explosion()
        {
            var cfg = GetItemEffect.mInstance;

            float angle = Random.Range(0, Mathf.PI * 2);
            float radius = Random.Range(cfg.mExplosionRadius.x, cfg.mExplosionRadius.y) * Random.Range(1,10);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Vector3 startPos = mTrans.position;
            Vector3 midPos = startPos + offset;
            float d1 = radius / (cfg.mExplosionSpeed * moveSpeed);
            float distToTarget = Vector3.Distance(midPos, mToWhere.position);
            float d2 = distToTarget / (cfg.mExplosionSpeed * moveSpeed);
            float totalTime = d1 + d2;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(mTrans.DOMove(midPos, d1).SetEase(Ease.OutCubic));
            sequence.Append(mTrans.DOMove(mToWhere.position, d2).SetEase(Ease.InCubic)).OnComplete(() =>
            {
                Punch(mToWhere);
            });
            sequence.AppendCallback(() =>
            {
                if (mArriveAction != null) mArriveAction();
                mIsInUse = false;
                gameObject.SetActive(false);
            });
            return totalTime;
        }
        public void Punch(Transform target, float scale = 1.5f, float duration = 0.15f)
        {
            if (target == null) return;
            target.DOKill();
            target.DOScale(oldscale*scale, duration).SetEase(Ease.OutBack).OnComplete(() => target.DOScale(oldscale, duration * 0.7f).SetEase(Ease.InBack));
        }
    }
}
