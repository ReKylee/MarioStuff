using System;
using UnityEngine;

namespace Collectables
{
    public class CoinCollectable : CollectibleBase
    {

        public override void OnCollect(GameObject collector)
        {
            OnCoinCollected?.Invoke();
        }
        public static event Action OnCoinCollected;
    }
}
