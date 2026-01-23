using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    private Dictionary<TKey, TValue> dict;

    public Dictionary<TKey, TValue> ToDictionary()
    {
        if (dict != null) return dict;

        dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];

        return dict;
    }
}