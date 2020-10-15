using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes an instance that can rotate.
    /// </summary>
    public interface IRotating
    {
        /// <summary>
        /// Gets the X-coordinate of the normal of the component (orientation).
        /// </summary>
        /// <value>
        /// The normal x-coordinate.
        /// </value>
        Function NormalX { get; }

        /// <summary>
        /// Gets the Y-coordinate of the normal of the component (orientation).
        /// </summary>
        /// <value>
        /// The normal y-coordinate.
        /// </value>
        Function NormalY { get; }

        /// <summary>
        /// Gets the angle.
        /// </summary>
        /// <value>
        /// The angle.
        /// </value>
        Function Angle { get; }
    }
}
