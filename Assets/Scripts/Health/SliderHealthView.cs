using Health.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Health
{
    public class SliderHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private Slider healthSlider;
        private Image _image;

        private void Awake()
        {
            _image = healthSlider.fillRect.GetComponent<Image>();
        }
        public void UpdateDisplay(int currentHp, int maxHp)
        {
            healthSlider.value = (float)currentHp / maxHp;
            healthSlider.maxValue = 1f;
            _image.color =
                Color.Lerp(Color.red, Color.green, (float)currentHp / maxHp);

        }
    }
}
