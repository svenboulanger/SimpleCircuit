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
    public class UpdateContext(IDiagnosticHandler diagnostics, IBiasingSimulationState state, IRelationshipContext relations) : IUpdateContext
    {
        private readonly IRelationshipContext _relationships = relations ?? throw new ArgumentNullException(nameof(relations));

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        /// <inheritdoc />
        public IBiasingSimulationState State { get; } = state ?? throw new ArgumentNullException(nameof(state));

        /// <inheritdoc />
        public List<WireSegment> WireSegments { get; } = [];

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
