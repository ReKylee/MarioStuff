using Interfaces;
using Managers;
using UnityEngine;

namespace Resettables
{

    public class ActiveResetter : MonoBehaviour, IResettable
    {
        [SerializeField] private bool activeOnReset = true;

        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }

        public void ResetState()
        {
            gameObject.SetActive(activeOnReset);
        }
    }
}
