using SimpleCircuit.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A component that describes a constraint between two nodes.
    /// </summary>
    public class OffsetConstraint : ICircuitSolverPresence
    {
        /// <inheritdoc />
        public int Order => 0;

        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the node that has the lowest value.
        /// </summary>
        public string Lowest { get; }

        /// <summary>
        /// Gets the node that has the highest value.
        /// </summary>
        public string Highest { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public double Offset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetConstraint"/> class.
        /// The constraint applies Left = Right + Offset
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        public OffsetConstraint(string name, string lowest, string highest, double offset = 0.0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            if (string.IsNullOrWhiteSpace(lowest))
                throw new ArgumentNullException(nameof(lowest));
            if (string.IsNullOrWhiteSpace(highest))
                throw new ArgumentNullException(nameof(highest));

            if (offset > 0)
            {
                Offset = offset;
                Lowest = lowest;
                Highest = highest;
            }
            else
            {
                Offset = -offset;
                Lowest = highest;
                Highest = lowest;
            }
        }

        /// <inheritdoc />
        public bool Reset(IDiagnosticHandler diagnostics) => true;

        /// <inheritdoc />
        public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
        {
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public bool DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            switch (context.Mode)
            {
                case NodeRelationMode.Offsets:
                    if (!context.Offsets.Group(Lowest, Highest, Offset))
                    {
                        diagnostics?.Post(ErrorCodes.CannotResolveFixedOffsetFor, Offset, Name);
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <inheritdoc />
        public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }
    }
}
