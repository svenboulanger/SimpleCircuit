using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a component with a graphical representation, and pins that can be interconnected.
    /// </summary>
    /// <seealso cref="IItem" />
    public interface IComponent
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the x-coordinate of the component.
        /// </summary>
        /// <value>
        /// The x-coordinate.
        /// </value>
        Function X { get; }

        /// <summary>
        /// Gets the y-coordinate of the component.
        /// </summary>
        /// <value>
        /// The y-coordinate.
        /// </value>
        Function Y { get; }

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
        /// Gets the mirror scale (in case the component is mirrored).
        /// </summary>
        /// <value>
        /// The mirror scale.
        /// </value>
        Function MirrorScale { get; }

        /// <summary>
        /// Gets the pins.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        PinCollection Pins { get; }

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        /// <param name="drawing">The drawing.</param>
        void Render(SvgDrawing drawing);

        /// <summary>
        /// Applies some functions to the minimizer if necessary.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        void Apply(Minimizer minimizer);
    }
}
