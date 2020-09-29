using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A 2D element that can be mirrorred.
    /// </summary>
    public interface IScaling
    {
        /// <summary>
        /// Gets the mirror scale (in case the component is mirrored).
        /// </summary>
        /// <value>
        /// The mirror scale.
        /// </value>
        Function Scale { get; }
    }
}
