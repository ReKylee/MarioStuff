using Health;
using Interfaces.Damage;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class SliderHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private GameObject target; // Assign the player GameObject


        private void OnEnable()
        {
            if (target && target.TryGetComponent(out IDamageable damageable))
            {
                damageable.OnHealthChanged += UpdateDisplay;
                UpdateDisplay(damageable.CurrentHp, damageable.MaxHp);
            }
        }

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            healthSlider.value = (float)currentHp / maxHp;
            healthSlider.maxValue = 1f;
            healthSlider.fillRect.GetComponent<Image>().color =
                Color.Lerp(Color.red, Color.green, (float)currentHp / maxHp);

        }
    }
}
