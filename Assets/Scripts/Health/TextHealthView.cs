using TMPro;
using UnityEngine;

namespace Health
{
    public class TextHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private TextMeshProUGUI text;

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            text.text = $"{currentHp}/{maxHp}";
        }
    }
}
