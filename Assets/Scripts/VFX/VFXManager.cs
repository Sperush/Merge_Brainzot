using System.Collections.Generic;
using UnityEngine;
public enum VFXType
{
    WinFirework,
    Merge,
    Hit,
    Spawn,
    Bomp,
    Freeze
}

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [System.Serializable]
    public class VFXItem
    {
        public VFXType type;
        public ParticleSystem prefab;
        public int preloadCount;
    }

    public List<VFXItem> vfxList;
    private Dictionary<VFXType, Queue<ParticleSystem>> pool =
        new Dictionary<VFXType, Queue<ParticleSystem>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPool();
        }
        else Destroy(gameObject);
    }

    void InitPool()
    {
        foreach (var item in vfxList)
        {
            Queue<ParticleSystem> q = new Queue<ParticleSystem>();

            for (int i = 0; i < item.preloadCount; i++)
            {
                var vfx = Instantiate(item.prefab);
                vfx.gameObject.SetActive(false);
                q.Enqueue(vfx);
            }

            pool[item.type] = q;
        }
    }
    public void Play(VFXType type, Vector3 position)
    {
        if (!pool.ContainsKey(type)) return;

        var q = pool[type];
        ParticleSystem vfx;

        if (q.Count > 0)
        {
            vfx = q.Dequeue();
        }
        else
        {
            vfx = Instantiate(GetPrefab(type));
        }

        vfx.transform.position = position;
        vfx.gameObject.SetActive(true);
        vfx.Play();

        StartCoroutine(ReturnToPool(type, vfx));
    }

    ParticleSystem GetPrefab(VFXType type)
    {
        return vfxList.Find(x => x.type == type).prefab;
    }
    System.Collections.IEnumerator ReturnToPool(VFXType type, ParticleSystem vfx)
    {
        yield return new WaitForSeconds(vfx.main.duration);
        vfx.gameObject.SetActive(false);
        pool[type].Enqueue(vfx);
    }
}

