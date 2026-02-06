using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; // Dùng DOTween để làm hiệu ứng dừng kim cho đẹp

namespace GIE
{
    public class LuckyGauge : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform needleRect;
        public TMP_Text[] txt;
        [Header("Settings")]
        public float rotationSpeed = 3f;
        public float maxAngle = 30f;

        private float mCurrentTime = 0f;
        private int oldMultip = -1;
        void Update()
        {
            if (BattleManager.Instance.mIsMoving)
            {
                mCurrentTime += Time.deltaTime * rotationSpeed;
                float angle = Mathf.Sin(mCurrentTime) * maxAngle;

                // Xoay kim theo trục Z
                needleRect.localRotation = Quaternion.Euler(0, 0, angle);
                int multiplier = GetMultiplierByAngle(needleRect.localEulerAngles.z);
                if (multiplier != oldMultip)
                {
                    oldMultip = multiplier;
                    if (BattleManager.Instance.winPanel.activeSelf)
                    {
                        txt[0].SetText(Char.FormatMoney(BattleManager.Instance.coinReward * multiplier));
                    }
                    else
                    {
                        txt[0].SetText(Char.FormatMoney(BattleManager.Instance.coinReward * multiplier));
                    }
                }
            }
        }

        public void OnClickStop()
        {
            if (!BattleManager.Instance.mIsMoving) return;
            BattleManager.Instance.mIsMoving = false;
            int multiplier = GetMultiplierByAngle(needleRect.localEulerAngles.z);

            Debug.Log($"Kim dừng tại góc: {needleRect.localEulerAngles.z} -> Nhân: x{multiplier}");

             //Gọi xem quảng cáo
             //Nếu có AdManager thì dùng dòng dưới, nếu test thì gọi thẳng Reward
            RewardedAds.Instance.LoadRewardedAd((isSuccess) =>
            {
                if (isSuccess)
                {
                    GiveReward(multiplier);
                    Debug.Log("Đã cộng tiền thành công!");
                }
                else
                {
                    Debug.Log("Người chơi tắt ngang hoặc lỗi Ad, không thưởng.");
                }
            });
        }
        int GetMultiplierByAngle(float angleZ)
        {
            if (angleZ > 180) angleZ -= 360;
            float absAngle = Mathf.Abs(angleZ);
            if (absAngle <= 7.7f)
            {
                return 5;
            }
            else if (absAngle <= 19.6f)
            {
                return 3;
            }
            else
            {
                return 2;
            }
        }

        void GiveReward(int multiplier)
        {
            long totalCoins = BattleManager.Instance.coinReward * multiplier;
            // Hiệu ứng "Punch" cái kim một cái cho nảy nảy
            needleRect.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).OnComplete(() => needleRect.DOScale(1f, 0.1f));
            Char.Instance.AddCoins(totalCoins);
            if (!BattleManager.Instance.winPanel.activeSelf) BattleManager.Instance.ResetLV();
            else BattleManager.Instance.ChangeLV();
            oldMultip = -1;
        }
    }
}