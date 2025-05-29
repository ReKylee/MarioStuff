using Health;
using Interfaces.Damage;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class SliderHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private IDamageable _damageable;

        private void OnEnable()
        {
            _damageable.OnHealthChanged += UpdateDisplay;
        }

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            _healthSlider.value = (float)currentHp / maxHp;
            _healthSlider.maxValue = 1f;
            _healthSlider.fillRect.GetComponent<Image>().color =
                Color.Lerp(Color.red, Color.green, (float)currentHp / maxHp);

        }
    }
}
