using Collectables.Base;
using TMPro;
using UnityEngine;

namespace Collectables
{
    public class CoinView : MonoBehaviour, ICounterView
    {
        [SerializeField] private TextMeshProUGUI coinText;

        public void UpdateCountDisplay(int count)
        {
            if (coinText) coinText.text = $"Coins: {count}";
        }
    }
}
