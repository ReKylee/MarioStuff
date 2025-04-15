using Interfaces;
using UnityEngine;

public class FireballWeapon : MonoBehaviour, IUseableWeapon
{
    public GameObject fireball;
    private bool _isEquip;


    public void Shoot()
    {
        if (fireball && _isEquip)
        {
            GameObject curFireball = Instantiate(fireball, transform.position, new Quaternion());
            ProjectileFireball scFireball = curFireball.GetComponent<ProjectileFireball>();
            if (scFireball)
            {
                float direction = transform.parent?.localScale.x ?? 1;
                scFireball.Shoot(direction);
            }
        }
    }

    public void UnEquip()
    {
        _isEquip = false;
    }
    public void Equip()
    {
        _isEquip = true;
    }
}
