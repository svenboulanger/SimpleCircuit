using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;

namespace SimpleCircuit.Components.Constraints
{
    /// <summary>
    /// A constraint placed on the orientation of a pin.
    /// </summary>
    public class PinOrientationConstraint : ICircuitPresence
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 0;

        /// <summary>
        /// Gets the pin information.
        /// </summary>
        public PinInfo Pin { get; }

        /// <summary>
        /// Gets the default index if no pin is specified.
        /// A negative value will count from the last pin down.
        /// </summary>
        public int DefaultIndex { get; }

        /// <summary>
        /// Gets the orientation that the pin needs to be constrained to.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Creates a new <see cref="PinOrientationConstraint"/>.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="drawable">The name of the drawable.</param>
        /// <param name="pin">The name of the pin.</param>
        /// <param name="defaultIndex">The default pin index if no pin is given.</param>
        /// <param name="orientation">The orientation of the pin.</param>
        public PinOrientationConstraint(string name, PinInfo pin, int defaultIndex, Vector2 orientation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pin = pin;
            DefaultIndex = defaultIndex;
            Orientation = orientation;
        }

        /// <inheritdoc />
        public void Reset() { }

        /// <inheritdoc />
        public void Prepare(GraphicalCircuit circuit, IDiagnosticHandler diagnostics)
        {
            var drawable = Pin.Component.Component;
            IPin pin;
            if (Pin.Pin.Content.Length == 0)
            {
                // Use pin index
                int index = DefaultIndex >= 0 ? DefaultIndex : drawable.Pins.Count + DefaultIndex;
                pin = drawable.Pins[index];
            }
            else
            {
                if (!drawable.Pins.TryGetValue(Pin.Pin.Content.ToString(), out pin))
                {
                    diagnostics?.Post(Pin.Component.Name, ErrorCodes.CouldNotFindPin, Pin.Pin.Content, drawable.Name);
                    return;
                }
            }

            if (pin is IOrientedPin op)
                op.ResolveOrientation(Orientation, diagnostics);
        }
    }
}
