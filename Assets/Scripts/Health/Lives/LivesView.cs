using TMPro;
using UnityEngine;

namespace Health.Lives
{
    public class LivesView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private LivesController controller;

        private void OnEnable()
        {
            if (controller?.Lives != null)
            {
                controller.Lives.OnChanged += UpdateDisplay;
            }
        }

        private void OnDisable()
        {
            if (controller?.Lives != null)
            {
                controller.Lives.OnChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(int current, int max)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {current}/{max}";
            }
        }
    }
}
