using Collectables.Base;
using Collectables.Counter;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Collectables
{
    public class CoinController : MonoBehaviour, IResettable
    {
        [SerializeField] private CoinView view; // Dependency injection via inspector

        private ICounterModel _model;

        private void Awake()
        {
            _model = new CounterModel(); // Depends on abstraction
            ConnectModelToView();
            RegisterForReset();
            SubscribeToEvents();
        }

        private void ConnectModelToView()
        {
            _model.OnCountChanged += view.UpdateCountDisplay;
        }

        private void RegisterForReset()
        {
            ResetManager.Instance?.Register(this);
        }

        private void SubscribeToEvents()
        {
            CoinCollectable.OnCoinCollected += _model.Increment;
        }

        public void ResetState() => _model.Reset();

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnCountChanged -= view.UpdateCountDisplay;
                CoinCollectable.OnCoinCollected -= _model.Increment;
            }
        }
    }
}
