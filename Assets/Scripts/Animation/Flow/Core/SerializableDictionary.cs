using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Serializable dictionary for Unity serialization
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeReference] private List<TKey> keys = new();
        [SerializeReference] private List<TValue> values = new();

        /// <summary>
        ///     Create an empty serializable dictionary
        /// </summary>
        public SerializableDictionary()
        {
        }

        /// <summary>
        ///     Create a serializable dictionary from an existing dictionary
        /// </summary>
        public SerializableDictionary(Dictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        /// <summary>
        ///     Serialize dictionary to lists before Unity serialization
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        /// <summary>
        ///     Deserialize lists back to dictionary after Unity serialization
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}
