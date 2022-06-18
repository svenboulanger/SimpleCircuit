using SimpleCircuit.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A constraint between two nodes that tries to guarantee a minimum between the two.
    /// </summary>
    public class MinimumConstraint : ICircuitPresence
    {
        public const string DiodeModelName = "#MinimumOffsetPinModel";

        /// <summary>
        /// Adds the diode model that can be used to define minimum constraints.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        public static void AddRectifyingModel(IEntityCollection circuit)
        {
            if (circuit.Contains(DiodeModelName))
                return;
            var model = new VoltageSwitchModel(DiodeModelName);
            model.Parameters.OnResistance = 1e-6;
            model.Parameters.OffResistance = 1e9;
            model.Parameters.Hysteresis = 1e-3;
            model.Parameters.Threshold = 0.0;
            circuit.Add(model);
        }

        /// <summary>
        /// Adds a structure to the circuit that tries to guarantee a minimum
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the elements.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The minimum between the two.</param>
        public static void AddMinimum(IEntityCollection circuit, string name, string lowest, string highest, double minimum, double weight = 1e-3)
        {
            string i = $"{name}.i";
            circuit.Add(new Resistor($"R{name}", highest, lowest, 1.0 / weight));
            circuit.Add(new VoltageSource($"V{name}", i, lowest, minimum));
            AddRectifyingElement(circuit, $"D{name}", i, highest);
        }

        /// <summary>
        /// Adds an idealized diode. I.e. an element that conducts if <paramref name="from"/> is
        /// higher than <paramref name="to"/>.
        /// </summary>
        /// <remarks>
        /// This isn't really a diode, but a voltage-controlled switch. But for thinking it helps to look at it
        /// like a diode.
        /// </remarks>
        /// <param name="circuit">The circuit to add the diode to.</param>
        /// <param name="name">The name of the diode.</param>
        /// <param name="from">The name of the node where currents flows away.</param>
        /// <param name="to">The name of the node where current flows into.</param>
        public static void AddRectifyingElement(IEntityCollection circuit, string name, string from, string to)
        {
            AddRectifyingModel(circuit);
            circuit.Add(new VoltageSwitch(name, from, to, from, to, DiodeModelName));
        }

        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the lowest of the two nodes.
        /// </summary>
        public string Lowest { get; }

        /// <summary>
        /// Gets the highest of the two nodes.
        /// </summary>
        public string Highest { get; }

        /// <summary>
        /// Gets the minimum between the two nodes.
        /// </summary>
        public double Minimum { get; }

        /// <summary>
        /// Creates a new <see cref="MinimumConstraint"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The minimum between the two values.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MinimumConstraint(string name, string lowest, string highest, double minimum)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            if (string.IsNullOrWhiteSpace(lowest))
                throw new ArgumentNullException(nameof(lowest));
            Lowest = lowest;
            if (string.IsNullOrWhiteSpace(highest))
                throw new ArgumentNullException(nameof(highest));
            Highest = highest;
            Minimum = minimum;
        }

        /// <inheritdoc />
        public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public void Register(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            var highest = context.Nodes.Shorts[Highest];
            var lowest = context.Nodes.Shorts[Lowest];
            if (highest != lowest)
                AddMinimum(context.Circuit, Name, lowest, highest, Minimum);
        }

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics)
        {
        }
    }
}
