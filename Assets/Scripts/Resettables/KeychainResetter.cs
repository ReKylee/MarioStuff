using Controller;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Resettables
{
    public class KeychainResetter : MonoBehaviour, IResettable
    {
        private KeychainController _keychainController;
        private void Start()
        {
            _keychainController = GetComponent<KeychainController>();
            ResetManager.Instance?.Register(this);

        }

        public void ResetState()
        {
            _keychainController.KeysManager.Reset();
        }
    }
}
