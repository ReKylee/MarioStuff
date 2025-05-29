using Interfaces.Damage;
using TMPro;
using UnityEngine;

namespace Health
{
    public class TextHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private IDamageable _damageable;
        private void OnEnable()
        {
            _damageable.OnHealthChanged += UpdateDisplay;
        }
        public void UpdateDisplay(int currentHp, int maxHp)
        {
            _text.text = $"{currentHp}/{maxHp}";
        }
    }
}
