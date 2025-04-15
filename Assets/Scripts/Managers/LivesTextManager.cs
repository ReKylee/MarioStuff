using Controller;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class LivesTextManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private HealthController health;
        private void OnEnable()
        {
            health.OnHealthChanged += UpdateLivesText;
        }
        private void OnDisable()
        {
            health.OnHealthChanged -= UpdateLivesText;
        }
        private void UpdateLivesText(int lives, int maxLives)
        {
            livesText.text = $"HP: {lives} / {maxLives}";
        }
    }
}
