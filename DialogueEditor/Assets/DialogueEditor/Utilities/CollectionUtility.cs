using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> _serializableDictionary, K _key, V _value)
        {
            if (_serializableDictionary.ContainsKey(_key))
            {
                _serializableDictionary[_key].Add(_value);
                return;
            }
            _serializableDictionary.Add(_key, new List<V>() { _value });
        }
    }
}