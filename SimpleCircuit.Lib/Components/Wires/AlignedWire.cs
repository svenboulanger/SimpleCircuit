using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire that simply alignes multiple circuit presences.
    /// </summary>
    public class AlignedWire : ICircuitSolverPresence
    {
        private readonly IEnumerable<ILocatedPresence> _presences;
        private readonly Axis _axis;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 2;

        /// <summary>
        /// Creates a new <see cref="AlignedWire"/>.
        /// </summary>
        /// <param name="name">The name of the alignment.</param>
        /// <param name="presences">The presences that need to be aligned.</param>
        /// <param name="axis">The axis along which the alignment takes place.</param>
        public AlignedWire(string name, IEnumerable<ILocatedPresence> presences, Axis axis)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            _presences = presences ?? throw new ArgumentNullException(nameof(presences));
            _axis = axis;
        }

        /// <inheritdoc />
        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:
                    bool doX = (_axis & Axis.X) != 0;
                    bool doY = (_axis & Axis.Y) != 0;
                    ILocatedPresence last = null;
                    foreach (var presence in _presences)
                    {
                        if (last != null)
                        {
                            if (doX && !context.Offsets.Group(last.X, presence.X, 0.0))
                            {
                                throw new Exception();
                            }
                            if (doY && !context.Offsets.Group(last.Y, presence.Y, 0.0))
                            {
                                throw new Exception();
                            }
                        }
                        last = presence;
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
            => PresenceResult.Success;
    }
}
