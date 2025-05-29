using Health;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class SliderHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private Slider healthSlider;

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            healthSlider.value = (float)currentHp / maxHp;
            healthSlider.maxValue = 1f;
            healthSlider.fillRect.GetComponent<Image>().color =
                Color.Lerp(Color.red, Color.green, (float)currentHp / maxHp);

        }
    }
}
