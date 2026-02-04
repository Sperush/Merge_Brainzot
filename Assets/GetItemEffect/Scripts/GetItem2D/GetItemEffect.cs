using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting; // Đảm bảo bạn đã import DOTween

namespace GIE
{
    public enum TypeItem
    {
        Coins,
        Gems
    }

    [Serializable]
    public class Item
    {
        public TypeItem mItemName;
        public GetItem mItemTemplate;
        public Transform mItemToWhere;
        public int mCacheNumber = 20;
        [HideInInspector] // Ẩn list này trong inspector để tránh rối
        public List<GetItem> mItems = new List<GetItem>();
    }

    public class GetItemEffect : MonoBehaviour
    {
        public static GetItemEffect mInstance;
        public Canvas canvas;
        public RectTransform canvasRect;
        public float BaseSize => canvasRect.rect.width;
        // --- Explosion Variables (Đã sửa lỗi chính tả) ---
        [Header("Explosion Settings")]
        [Tooltip("Random selection in the range X and Y")]
        public Vector2 mExplosionRadius = new Vector2(0.1f, 0.15f);
        [Tooltip("Spread speed")]
        public float mExplosionSpeed = 0.5f;
        [Tooltip("The speed of the flight towards the target point")]
        public float mExplosionFlySpeed = 2.5f;

        // --- Jump Variables ---
        [HideInInspector] // Sẽ được Editor vẽ, ẩn ở chế độ default
        public Vector2 mJumpRadius = new Vector2(0.05f, 0.2f);
        [HideInInspector]
        public Vector2 mJumpHeight = new Vector2(0.1f, 0.25f);
        [HideInInspector]
        public float mJumpToFlyDuration = 0.3f;
        [HideInInspector]
        public float mJumpSpeed = 0.4f;
        [HideInInspector]
        public float mJumpFlySpeed = 2.5f;

        // --- Fly Variables ---
        [HideInInspector]
        public Vector2 mFlyRadius = new Vector2(0.2f, 0.4f);
        [HideInInspector]
        public float mFlySpeed = 1.5f;

        public List<Item> mGetItem = new List<Item>();

        void Awake()
        {
            mInstance = this;
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        void OnDestroy()
        {
            mInstance = null;
        }
        void Start()
        {
            for (int i = 0; i < mGetItem.Count; i++)
            {
                if (mGetItem[i].mItemTemplate == null) continue;

                mGetItem[i].mItemTemplate.gameObject.SetActive(false);
                mGetItem[i].mItems.Clear();
                for (int j = 0; j < mGetItem[i].mCacheNumber; j++)
                    CreateItem(i);
            }
        }

        public GetItem CreateItem(int index)
        {
            if (mGetItem[index].mItemTemplate == null) return null;

            GetItem item = Instantiate(mGetItem[index].mItemTemplate, mGetItem[index].mItemTemplate.transform.parent);
            item.gameObject.SetActive(false);
            mGetItem[index].mItems.Add(item);
            return item;
        }

        public float GetItem(TypeItem item_name, int item_number, Transform from_where = null, Transform to_where = null, System.Action item_arrive_action = null)
        {
            return GetItem(item_name, item_number, from_where != null ? from_where.position:Vector3.zero, to_where, item_arrive_action);
        }

        public float GetItem(TypeItem item_name, int item_number, Vector3 from_where, Transform to_where = null, System.Action item_arrive_action = null)
        {
            for (int i = 0; i < mGetItem.Count; i++)
            {
                if (mGetItem[i].mItemName.Equals(item_name))
                {
                    return GetItem(mGetItem[i], i, item_number, from_where, to_where, item_arrive_action);
                }
            }
            return 0f;
        }

        public float GetItem(Item item_config, int index, int item_number, Vector3 from_where, Transform to_where, System.Action item_arrive_action)
        {
            float maxTime = 0f;
            int use_count = 0;
            for (int i = 0; i < item_config.mItems.Count; i++)
            {
                if (item_config.mItems[i].mIsInUse == false && use_count < item_number)
                {
                    use_count++;
                    float time = item_config.mItems[i].PlayEffect(from_where, to_where == null ? item_config.mItemToWhere : to_where, item_arrive_action);
                    maxTime = Mathf.Max(maxTime, time);
                }
            }
            return maxTime;
        }
    }
}