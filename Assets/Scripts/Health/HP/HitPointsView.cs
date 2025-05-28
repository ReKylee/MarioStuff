using TMPro;
using UnityEngine;

namespace Health.HP
{
    public class HitPointsView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private HitPointController controller;

        private void OnEnable()
        {
            if (controller?.Health != null)
            {
                controller.Health.OnChanged += UpdateDisplay;
            }
        }

        private void OnDisable()
        {
            if (controller?.Health != null)
            {
                controller.Health.OnChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(int current, int max)
        {
            if (healthText != null)
            {
                healthText.text = $"HP: {current}/{max}";
            }
        }
    }
}
