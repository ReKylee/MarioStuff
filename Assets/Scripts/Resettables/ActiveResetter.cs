using Interfaces;
using Managers;
using UnityEngine;

namespace Resettables
{
    public class ActiveResetter : MonoBehaviour, IResettable
    {
        private bool _initialActive;
        private void Start()
        {
            _initialActive = gameObject.activeSelf;
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            gameObject.SetActive(_initialActive);
        }
    }
}
