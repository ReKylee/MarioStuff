using UnityEngine;

namespace Interfaces.Collectibles
{
    public interface ICollectable
    {

        void OnCollect(GameObject collector);
    }
}
