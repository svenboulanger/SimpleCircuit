using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of the <see cref="IUpdateContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="UpdateContext"/>.
    /// </remarks>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <param name="state">The state.</param>
    /// <param name="relations">The relationships between nodes.</param>
    public class UpdateContext(IDiagnosticHandler diagnostics, IBiasingSimulationState state, IPrepareContext relations) : IUpdateContext
    {
        private readonly Dictionary<string, double> _unmapped = [];

        private readonly IPrepareContext _relationships = relations ?? throw new ArgumentNullException(nameof(relations));

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        /// <inheritdoc />
        public IBiasingSimulationState State { get; } = state;

        /// <inheritdoc />
        public List<WireSegment> WireSegments { get; } = [];

        /// <inheritdoc />
        public double GetValue(string node)
        {
            _relationships.Offsets.TryGet(node, out string representative, out double offset);
            if (State is not null && State.TryGetValue(representative, out var value))
                return value.Value + offset;
            
            // It's an unmapped value
            if (!_unmapped.TryGetValue(representative, out double existing))
            {
                existing = 0.0;
                _unmapped.Add(representative, 0.0);
            }
            return existing + offset;
        }

        /// <inheritdoc />
        public Vector2 GetValue(string nodeX, string nodeY)
            => new(GetValue(nodeX), GetValue(nodeY));

        /// <summary>
        /// Adds an offset to a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="offset">The offset.</param>
        public void AddOffset(string node, double offset)
        {
            var representative = _relationships.Offsets.GetRepresentative(node);
            if (State is not null && State.TryGetValue(representative, out var variable))
            {
                int index = State.Map[variable];
                State.Solution[index] += offset;
            }
            else
            {
                if (!_unmapped.TryGetValue(representative, out double existing))
                    existing = 0.0;
                _unmapped[representative] = existing + offset;
            }
        }
    }
}
