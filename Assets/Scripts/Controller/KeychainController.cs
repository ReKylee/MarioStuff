using Collectables;
using Interfaces;
using Managers;
using UnityEngine;

namespace Controller
{
    public class KeychainController : MonoBehaviour
    {
        public KeysManager KeysManager { get; private set; }

        private void Awake()
        {
            KeysManager = new KeysManager();
        }
        private void OnEnable()
        {
            KeyCollectable.OnKeyCollected += CollectKey;
        }
        private void OnDisable()
        {
            KeyCollectable.OnKeyCollected -= CollectKey;
        }
        private void CollectKey(IKey key)
        {
            KeysManager?.PickUpKey(key);
        }
    }
}
