using System;
using Interfaces.Locks;
using UnityEngine;

namespace Collectables
{
    public class KeyCollectable : CollectibleBase
    {
        private IKey _myKey;
        private void Awake()
        {
            _myKey = GetComponent<IKey>();
        }

        public override void OnCollect(GameObject collector)
        {
            if (_myKey == null) return;

            OnKeyCollected?.Invoke(_myKey);
        }
        public static event Action<IKey> OnKeyCollected;
    }
}
