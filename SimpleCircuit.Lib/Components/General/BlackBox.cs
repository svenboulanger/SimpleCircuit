﻿using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A generic black box.
    /// </summary>
    [SimpleKey("BB", "A black box. Pins are created on the fly, where the first character (n, s, e, w) indicate the side.", Category = "General")]
    public partial class BlackBox : ILocatedDrawable, ILabeled
    {
        private PinCollection _pins { get; }

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => _pins;

        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; protected set; }

        /// <inheritdoc />
        public Vector2 EndLocation { get; protected set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Creates a new black box.
        /// </summary>
        /// <param name="name">The name.</param>
        public BlackBox(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            _pins = new(this);
            X = $"{Name}.x";
            Y = $"{Name}.y";
            EndLocation = new(20, 20);
        }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            drawing.StartGroup(Name, GetType().Name.ToLower());
            drawing.Polygon(new[]
            {
                Location, new Vector2(EndLocation.X, Location.Y),
                EndLocation, new Vector2(Location.X, EndLocation.Y)
            });

            // Draw the port names
            _pins.Render(drawing);
            drawing.EndGroup();
        }

        /// <inheritdoc />
        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            _pins.DiscoverNodeRelationships(context, diagnostics);
        }

        /// <inheritdoc />
        public void Register(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            _pins.Register(context, diagnostics);
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics)
        {
            var map = context.Nodes.Shorts;
            double x, y;
            if (state.TryGetValue(map[X], out var value))
                x = value.Value;
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "U001", $"Could not find variable '{X}'."));
                x = 0.0;
            }
            if (state.TryGetValue(map[Y], out value))
                y = value.Value;
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "U001", $"Could not find variable '{X}'."));
                y = 0.0;
            }
            Location = new(x, y);

            if (state.TryGetValue(map[_pins.Right], out value))
                x = value.Value;
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "U001", $"Could not find variable '{X}'."));
                x = 0.0;
            }
            if (state.TryGetValue(map[_pins.Bottom], out value))
                y = value.Value;
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "U001", $"Could not find variable '{X}'."));
                y = 0.0;
            }
            EndLocation = new(x, y);

            // Update all pin locations as well
            // We ignore pin 0, because that is a dummy pin
            for (int i = 1; i < _pins.Count; i++)
                _pins[i].Update(state, context, diagnostics);
        }
    }
}