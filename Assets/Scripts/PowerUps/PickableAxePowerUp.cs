using Interfaces;
using UnityEngine;

public class PickableAxePowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        Debug.Log("ApplyPowerUp Fire Flower");
        if (player != null)
        {
            AxeWeapon axeWeapon = player.GetComponentInChildren<AxeWeapon>();
            if (axeWeapon != null)
                axeWeapon.Reload();
        }
    }
}
