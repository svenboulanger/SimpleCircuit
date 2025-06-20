﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Constraints
{
    /// <summary>
    /// A constraint placed on the orientation of a pin.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="PinOrientationConstraint"/>.
    /// </remarks>
    /// <param name="name">The name of the constraint.</param>
    /// <param name="pin">The name of the pin.</param>
    /// <param name="defaultIndex">The default pin index if no pin is given.</param>
    /// <param name="segment">The orientation of the pin.</param>
    public class PinOrientationConstraint(string name, PinReference pin, int defaultIndex, WireSegmentInfo segment, bool invert) : ICircuitPresence
    {
        /// <inheritdoc />
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <inheritdoc />
        /// <remarks>
        /// We want pin orientation constraints to happen first.
        /// </remarks>
        public int Order => -2;

        /// <summary>
        /// Gets the pin information.
        /// </summary>
        public PinReference Pin { get; } = pin;

        /// <summary>
        /// Gets the default index if no pin is specified.
        /// A negative value will count from the last pin down.
        /// </summary>
        public int DefaultIndex { get; } = defaultIndex;

        /// <summary>
        /// Gets the wire segment that needs to be constrained with.
        /// </summary>
        public WireSegmentInfo Segment { get; } = segment;

        /// <summary>
        /// Gets whether the orientation needs to be inverted compared to the wire segment.
        /// </summary>
        public bool Invert { get; } = invert;

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            if (context.Mode == PreparationMode.Orientation)
            {
                // Get the drawable
                var drawable = Pin.Drawable;
                if (drawable is null)
                    return PresenceResult.Success;

                // Get the pin
                var pin = Pin.GetOrCreate(context.Diagnostics, DefaultIndex);
                if (pin is null)
                    return PresenceResult.GiveUp;

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
                    op.ResolveOrientation(orientation, Segment.Source, context.Diagnostics);
                }
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Fail(IDiagnosticHandler diagnostics)
        {
            var drawable = Pin.Drawable;
            if (drawable is null)
                return;

            if (Pin.Name.Length > 0)
            {
                if (!drawable.Pins.TryGetValue(Pin.Name, out _))
                    diagnostics?.Post(Pin.Source, ErrorCodes.CouldNotFindPin, Pin.Name, drawable.Name);
            }
        }
    }
}
