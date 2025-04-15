using TMPro;
using UnityEngine;

public class AxesManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI axesText;

    private void OnEnable()
    {
        AxeWeapon.OnAxeCollected += OnAxeCollision;
        AxeWeapon.OnAxeFired += OnAxeFired;

    }

    private void OnDisable()
    {
        AxeWeapon.OnAxeCollected -= OnAxeCollision;
        AxeWeapon.OnAxeFired -= OnAxeFired;
    }

    private void OnAxeCollision(int axes)
    {
        axesText.text = $"Axes: {axes}";
    }

    private void OnAxeFired(int axes)
    {
        axesText.text = $"Axes: {axes}";
    }
}
