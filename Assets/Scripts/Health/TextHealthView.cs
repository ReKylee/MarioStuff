using Interfaces.Damage;
using TMPro;
using UnityEngine;

namespace Health
{
    public class TextHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private GameObject target;


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
            text.text = $"{currentHp}/{maxHp}";
        }
    }
}
