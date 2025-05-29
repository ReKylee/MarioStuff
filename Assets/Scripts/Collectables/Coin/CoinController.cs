using System;
using Collectables.Base;
using Collectables.Counter;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Collectables
{
    public class CoinController : MonoBehaviour, IResettable
    {
        [SerializeField] private CoinView view;

        private ICounterModel _model;

        private void Awake()
        {
            _model = new CounterModel();
            ResetManager.Instance?.Register(this);
        }
        private void OnEnable()
        {
            _model.OnCountChanged += view.UpdateCountDisplay;
            CoinCollectable.OnCoinCollected += _model.Increment;
        }
        private void OnDisable()
        {
            _model.OnCountChanged -= view.UpdateCountDisplay;
            CoinCollectable.OnCoinCollected -= _model.Increment;
        }

        public void ResetState() => _model.Reset();


    }
}
