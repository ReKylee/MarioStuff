using Collectables;
using Interfaces.Locks;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Controller
{
    public class KeychainController : MonoBehaviour, IResettable
    {
        public Keychain Keychain { get; private set; }

        private void Awake()
        {
            Keychain = new Keychain();
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
            Keychain?.PickUpKey(key);
        }
        public void ResetState() => Keychain.Reset();
    }
}
