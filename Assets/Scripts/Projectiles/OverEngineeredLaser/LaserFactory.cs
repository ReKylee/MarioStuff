using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{
    public class LaserFactory
    {
        public GameObject Create()
        {
            Debug.Log("LaserFactory: Received a request to create a laser.");
            return LaserPoolManager.Instance.Get();
        }
    }
}
