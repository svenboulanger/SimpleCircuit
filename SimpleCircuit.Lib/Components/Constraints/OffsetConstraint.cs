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
        /// <summary>
        /// Adds a structure to the circuit that tries to guarantee an offset between two nodes.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the elements.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="offset">The offset.</param>
        public static void AddOffset(IEntityCollection circuit, string name, string lowest, string highest, double offset)
        {
            // string i = $"{name}.i";
            // circuit.Add(new Resistor($"R{name}", i, highest, 1e-6));
            // circuit.Add(new VoltageSource($"V{name}", i, lowest, offset));

            // Northon equivalent has less unknowns to solve
            circuit.Add(new Resistor($"R{name}", highest, lowest, 1e-6));
            circuit.Add(new CurrentSource($"I{name}", lowest, highest, offset * 1e6));
        }

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
        public void Reset() { }

        /// <inheritdoc />
        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            if (Offset.IsZero())
                context.Shorts.Group(Lowest, Highest);
        }

        /// <inheritdoc />
        public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            var lowest = context.Nodes.Shorts[Lowest];
            var highest = context.Nodes.Shorts[Highest];
            if (lowest != highest)
                AddOffset(context.Circuit, $"constraint.{Name}", lowest, highest, Offset);
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
        }
    }
}
