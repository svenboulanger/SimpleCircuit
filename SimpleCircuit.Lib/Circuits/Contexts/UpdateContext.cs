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
    public class UpdateContext : IUpdateContext
    {
        private readonly IRelationshipContext _relationships;

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public IBiasingSimulationState State { get; }

        /// <inheritdoc />
        public List<WireSegment> WireSegments { get; } = new List<WireSegment>();

        /// <summary>
        /// Creates a new <see cref="UpdateContext"/>.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="state">The state.</param>
        /// <param name="relations">The relationships between nodes.</param>
        public UpdateContext(IDiagnosticHandler diagnostics, IBiasingSimulationState state, IRelationshipContext relations)
        {
            Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _relationships = relations ?? throw new ArgumentNullException(nameof(relations));
        }

        /// <inheritdoc />
        public double GetValue(string node)
        {
            var r = _relationships.Offsets[node];
            if (State.TryGetValue(r.Representative, out var value))
                return value.Value + r.Offset;
            return r.Offset;
        }

        /// <inheritdoc />
        public Vector2 GetValue(string nodeX, string nodeY)
            => new(GetValue(nodeX), GetValue(nodeY));
    }
}
