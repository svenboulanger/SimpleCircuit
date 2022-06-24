using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Components.Constraints
{
    /// <summary>
    /// A constraint placed on the orientation of a pin.
    /// </summary>
    public class PinOrientationConstraint : ICircuitPreparationPresence
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the entity name of which the pin needs to be constrained.
        /// </summary>
        public IOrientedPin Pin { get; }

        /// <summary>
        /// Gets the orientation that the pin needs to be constrained to.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Creates a new <see cref="PinOrientationConstraint"/>.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="pin">The pin.</param>
        /// <param name="orientation">The orientation of the pin.</param>
        public PinOrientationConstraint(string name, IOrientedPin pin, Vector2 orientation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pin = pin ?? throw new ArgumentNullException(nameof(pin));
            Orientation = orientation;
        }

        /// <inheritdoc />
        public void Reset() { }

        /// <inheritdoc />
        public void Prepare(GraphicalCircuit circuit, IDiagnosticHandler diagnostics)
        {
            Pin.ResolveOrientation(Orientation, diagnostics);
        }
    }
}
