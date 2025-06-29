using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{

    public class LaserDirector
    {
        private readonly LaserBuilder _builder;

        public LaserDirector(GameObject prefab)
        {
            _builder = new LaserBuilder(prefab);
        }


        public GameObject Construct() =>
            _builder
                .SetSpeed(15f)
                .SetDamage(1)
                .Build();


        public GameObject ConstructFastLaser() =>
            _builder
                .SetSpeed(30f)
                .SetDamage(1)
                .Build();
    }
}
