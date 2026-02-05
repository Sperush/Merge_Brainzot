using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public enum TypeBooster
{
    Freeze,
    Bomp
}
public class BoosterManager : MonoBehaviour
{
    public Image[] img;
    public GameObject[] booster;
    public TMP_Text[] txtCount;
    public static BoosterManager Instance;
    public ItemBoosterManager[] itemBoosters;
    public Image imgFreeze;
    public bool isOpenPanel;
    private void Awake()
    {
        Instance = this;
    }

    public void UseFreezeBooster(float duration = 5f)
    {
        if (!BattleManager.Instance.isOkPvP() || !Char.Instance.SubBooster(TypeBooster.Freeze)) return;
        foreach (var m in BattleManager.Instance.enemyTeam)
        {
            m.GetComponent<MonsterAI>().Freeze(duration);
        }
        imgFreeze.gameObject.SetActive(true);
        imgFreeze.DOKill();
        Color c = imgFreeze.color;
        c.a = 0.3f;
        imgFreeze.color = c;
        Sequence seq = DOTween.Sequence();
        seq.Append(imgFreeze.DOFade(1f, duration - 0.6f));
        //seq.AppendInterval();
        seq.Append(imgFreeze.DOFade(0.3f, 0.3f));
        seq.OnUpdate(() =>
        {
            if (!BattleManager.Instance.startPvP)
            {
                seq.Kill();
                imgFreeze.gameObject.SetActive(false);
            }
        });
        seq.OnComplete(() =>
        {
            imgFreeze.gameObject.SetActive(false);
        });
        AudioManager.Instance.Play(GameSound.freezeSound);
    }

    int GetStrongestMeleeMaxHP()
    {
        int maxHP = 0;

        foreach (var m in BattleManager.Instance.playerTeam)
        {
            MonsterHealth unit = m.GetComponent<MonsterHealth>();
            if (unit.stats.type == MonsterType.Melee &&
                unit.gameObject.activeSelf)
            {
                maxHP = Mathf.Max(maxHP, unit.stats.maxHP);
            }
        }
        return maxHP;
    }
    public void UseBombBooster()
    {
        if (!BattleManager.Instance.isOkPvP() || !Char.Instance.SubBooster(TypeBooster.Bomp)) return;
        int strongestMeleeHP = GetStrongestMeleeMaxHP();
        if (strongestMeleeHP <= 0) return;

        int damage = Mathf.RoundToInt(strongestMeleeHP * 0.5f);
        BombPlane plane = BombPlanePool.Instance.Get();
        plane.Init(() =>
        {
            AudioManager.Instance.Play(GameSound.bombSound);
            VFXManager.Instance.Play(VFXType.Bomp, Vector3.zero);
            ApplyBombDamage(damage);
        });
        // TODO: Plane animation + explosion VFX
    }
    void ApplyBombDamage(int damage)
    {
        var enemies = BattleManager.Instance.enemyTeam;

        for (int i = 0; i < enemies.Count; i++)
        {
            var hp = enemies[i].GetComponent<MonsterHealth>();
            if (hp != null && enemies[i].activeSelf)
            {
                hp.TakeDamage(damage);
            }
        }

        AudioManager.Instance.Play(GameSound.bombSound);
    }
}
