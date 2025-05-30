using Kirby.Interfaces;

namespace Kirby
{
    /// <summary>
    /// Interface for objects that can provide a transformation when swallowed by Kirby
    /// </summary>
    public interface ITransformationProvider
    {
        /// <summary>
        /// Gets the transformation this object provides when swallowed
        /// </summary>
        /// <returns>The transformation to apply to Kirby</returns>
        IKirbyTransformation GetTransformation();
    }
}
