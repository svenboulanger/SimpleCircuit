using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Wires;
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
        /// <remarks>
        /// We want pin orientation constraints to happen first.
        /// </remarks>
        public int Order => -2;

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
        /// Gets the wire segment that needs to be constrained with.
        /// </summary>
        public WireSegmentInfo Segment { get; }

        /// <summary>
        /// Gets whether the orientation needs to be inverted compared to the wire segment.
        /// </summary>
        public bool Invert { get; }

        /// <summary>
        /// Creates a new <see cref="PinOrientationConstraint"/>.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="pin">The name of the pin.</param>
        /// <param name="defaultIndex">The default pin index if no pin is given.</param>
        /// <param name="segment">The orientation of the pin.</param>
        public PinOrientationConstraint(string name, PinInfo pin, int defaultIndex, WireSegmentInfo segment, bool invert)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pin = pin;
            DefaultIndex = defaultIndex;
            Segment = segment;
            Invert = invert;
        }

        /// <inheritdoc />
        public void Reset() { }

        /// <inheritdoc />
        public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
        {
            // Get the drawable
            var drawable = Pin.Component.Component;
            if (drawable == null)
                return PresenceResult.Success;

            // Get the pin
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
                    if (mode == PresenceMode.Fix)
                    {
                        diagnostics?.Post(Pin.Pin, ErrorCodes.CouldNotFindPin, Pin.Pin.Content, Pin.Component.Fullname);
                        return PresenceResult.GiveUp;
                    }
                }
            }

            // Resolve the orientation of the found pin
            if (pin is IOrientedPin op)
            {
                var orientation = Segment.Orientation;
                if (orientation.X.IsZero() && orientation.Y.IsZero())
                    return PresenceResult.Success;

                if (Invert)
                    orientation = -orientation;

                // If there is no orientation, ignore constraining the pin (it may be that
                // the segment copies the orientation from the pin instead)
                op.ResolveOrientation(orientation, diagnostics);
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Fail(IDiagnosticHandler diagnostics)
        {
            var drawable = Pin.Component?.Component;
            if (drawable == null)
                return;

            if (Pin.Pin.Content.Length > 0)
            {
                if (!drawable.Pins.TryGetValue(Pin.Pin.Content.ToString(), out _))
                    diagnostics?.Post(Pin.Component.Name, ErrorCodes.CouldNotFindPin, Pin.Pin.Content, drawable.Name);
            }
        }
    }
}
