using Kirby.Interfaces;
using UnityEngine;

namespace Kirby
{
    /// <summary>
    ///     Component that provides a transformation when an enemy is swallowed by Kirby
    /// </summary>
    public class TransformationProvider : MonoBehaviour, ITransformationProvider
    {
        [Tooltip("The transformation that will be applied to Kirby when this enemy is swallowed")] [SerializeField]
        private KirbyTransformation transformation;

        /// <summary>
        ///     Gets the transformation this enemy provides when swallowed
        /// </summary>
        public IKirbyTransformation GetTransformation() => transformation;
    }
}
