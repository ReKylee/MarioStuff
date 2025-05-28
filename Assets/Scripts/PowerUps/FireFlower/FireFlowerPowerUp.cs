using Interfaces.PowerUps;
using UnityEngine;

public class FireFlowerPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        Debug.Log("ApplyPowerUp Fire Flower");
        if (player != null)
        {
            FireballWeapon fireballWeapon = player.GetComponentInChildren<FireballWeapon>();
            if (fireballWeapon != null)
                fireballWeapon.Equip();
        }
    }
}
