using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary : ISerializationCallbackReceiver
{
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private List<string> values = new List<string>();

    private Dictionary<string, string> dictionary = new Dictionary<string, string>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary.Clear();

        for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
        {
            dictionary[keys[i]] = values[i];
        }
    }

    public void FromDictionary(Dictionary<string, string> dict)
    {
        dictionary = new Dictionary<string, string>(dict);
    }

    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>(dictionary);
    }

    public bool ContainsKey(string key) => dictionary.ContainsKey(key);

    public string this[string key]
    {
        get => dictionary.TryGetValue(key, out string value) ? value : null;
        set => dictionary[key] = value;
    }

    public SerializableDictionary Clone()
    {
        var clone = new SerializableDictionary();
        clone.FromDictionary(this.dictionary);
        return clone;
    }
}