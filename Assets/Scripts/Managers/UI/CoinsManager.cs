using Collectables;
using Interfaces;
using Managers;
using TMPro;
using UnityEngine;

public class CoinsManager : MonoBehaviour, IResettable
{
    [SerializeField] private TextMeshProUGUI coinsText;
    private int _coins;

    private void Start()
    {
        ResetManager.Instance?.Register(this);
    }
    private void OnEnable()
    {
        CoinCollectable.OnCoinCollected += CoinCollected;
    }
    private void OnDisable()
    {
        CoinCollectable.OnCoinCollected -= CoinCollected;
    }
    public void ResetState()
    {
        _coins = 0;
        UpdateCoinsText();
    }

    private void CoinCollected()
    {
        _coins++;
        UpdateCoinsText();
    }
    private void UpdateCoinsText()
    {
        coinsText.text = $"Coins: {_coins}";
    }
}
