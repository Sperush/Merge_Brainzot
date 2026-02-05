using UnityEngine;
using System.Collections.Generic;

public class BombPlanePool : MonoBehaviour
{
    public static BombPlanePool Instance;

    public BombPlane prefab;
    private Queue<BombPlane> pool = new Queue<BombPlane>();

    void Awake()
    {
        Instance = this;
    }

    public BombPlane Get()
    {
        BombPlane plane;
        //if (pool.Count > 0)
        //{
        //    plane = pool.Dequeue();
        //}
        //else
        //{
            plane = Instantiate(prefab);
        //}
        return plane;
    }

    public void Release(BombPlane plane)
    {
        plane.StopAllCoroutines();
        plane.isEnd = true;
        StartCoroutine(plane.ResetVisuals(pool));
    }
}
