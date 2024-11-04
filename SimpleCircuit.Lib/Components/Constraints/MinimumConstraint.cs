using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Analog;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A constraint between two nodes that tries to guarantee a minimum between the two.
    /// </summary>
    public class MinimumConstraint : ICircuitSolverPresence
    {
        public const string DiodeModelName = "#MinimumOffsetPinModel";

        /// <inheritdoc />
        public int Order => 0;

        /// <summary>
        /// Adds a structure to the circuit that tries to guarantee a minimum
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the elements.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The weight of the minimum.</param>
        public static void AddMinimum(IEntityCollection circuit, string name, string lowest, string highest, double minimum, double weight = 1)
        {
            var component = new Constraints.MinimumConstraints.MinimumConstraint(name, highest, lowest, minimum);
            component.SetParameter("weight", weight);
            circuit.Add(component);
        }

        /// <summary>
        /// Adds a structure to the circuit that guarantees a minimum distance but keeps the order of the nodes depending on <paramref name="minimum"/>.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the elements.</param>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The end node.</param>
        /// <param name="minimum">The mininum.</param>
        /// <param name="weight">The weight of the minimum.</param>
        public static void AddDirectionalMinimum(IEntityCollection circuit, string name, string start, string end, double minimum, double weight = 1)
        {
            if (minimum > 0)
                AddMinimum(circuit, name, start, end, minimum, weight);
            else
                AddMinimum(circuit, name, end, start, -minimum, weight);
        }

        /// <summary>
        /// Adds a structure to the circuit that tries to guarantee a minimum between two relative items.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the structure.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The offset between the two nodes.</param>
        /// <param name="weight"></param>
        public static void AddMinimum(IEntityCollection circuit, string name, RelativeItem lowest, RelativeItem highest, double minimum, double weight = 1)
        {
            double delta = lowest.Offset - highest.Offset;
            AddMinimum(circuit, name, lowest.Representative, highest.Representative, delta + minimum, weight);
        }

        /// <summary>
        /// Adds a structure to the circuit that guarantees a minimum distance but keeps the order of the nodes depending on <paramref name="minimum"/>.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">A unique name for the elements.</param>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The end node.</param>
        /// <param name="minimum">The mininum.</param>
        /// <param name="weight">The weight of the minimum.</param>
        public static void AddDirectionalMinimum(IEntityCollection circuit, string name, RelativeItem start, RelativeItem end, double minimum, double weight = 1)
        {
            if (minimum > 0)
                AddMinimum(circuit, name, start, end, minimum, weight);
            else
                AddMinimum(circuit, name, end, start, -minimum, weight);
        }

        public static void AddMinimum(IEntityCollection circuit, string name,
            string fromX, string fromY, string toX, string toY,
            Vector2 offset, Vector2 normal, double minimum, double weight = 1.0)
        {
            var component = new Constraints.SlopedMinimumConstraints.SlopedMinimumConstraint(
                name, fromX, fromY, toX, toY, offset, normal, minimum);
            component.SetParameter("weight", weight);
            circuit.Add(component);
        }

        /// <summary>
        /// Adds a directional minimum in two directions.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="fromX">The start X-coordinate.</param>
        /// <param name="fromY">The start Y-coordinate.</param>
        /// <param name="toX">The end X-coordinate.</param>
        /// <param name="toY">The end Y-coordinate.</param>
        /// <param name="normal">The normal (direction).</param>
        /// <param name="minimum">The minimum distance along <paramref name="normal"/>.</param>
        public static void AddDirectionalMinimum(IEntityCollection circuit, string name,
            RelativeItem fromX, RelativeItem fromY, RelativeItem toX, RelativeItem toY,
            Vector2 normal, double minimum, double weight = 1.0)
        {
            if (minimum < 0.0)
            {
                minimum = -minimum;
                normal = -normal;
            }

            bool invertedX = normal.X < 0.0;
            bool invertedY = normal.Y < 0.0;
            var offset = new Vector2(fromX.Offset - toX.Offset, fromY.Offset - toX.Offset);
            var component = new Constraints.SlopedMinimumConstraints.SlopedMinimumConstraint(
                name, fromX.Representative, fromY.Representative, toX.Representative, toY.Representative, offset, normal, minimum);
            component.SetParameter("weight", weight);
            component.SetParameter("invertx", invertedX);
            component.SetParameter("inverty", invertedY);
            circuit.Add(component);
        }

        /// <summary>
        /// Computes a link for a minimum node.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The minimum.</param>
        public static bool MinimumLink(IRelationshipContext context, RelativeItem lowest, RelativeItem highest, double minimum)
        {
            double offset = lowest.Offset - highest.Offset + minimum;
            if (lowest.Representative != highest.Representative)
            {
                // We will try to see if this minimum allows us to order the representatives
                double delta = context.Offsets.GetBounds(highest.Representative).Minimum + context.Offsets.GetBounds(lowest.Representative).Maximum;
                if (offset > delta)
                    context.Extremes.Order(lowest.Representative, highest.Representative);
                else
                    context.Extremes.Linked.Group(lowest.Representative, highest.Representative);
                return true;
            }
            else
            {
                // Check whether the minimum is OK
                if (offset < 0 || offset.IsZero())
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Computes a link for a minimum node keeping the order depending on <paramref name="minimum"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="start">The start node.</param>
        /// <param name="end">The end node.</param>
        /// <param name="minimum">The minimum.</param>
        public static bool MinimumDirectionalLink(IRelationshipContext context, RelativeItem start, RelativeItem end, double minimum)
        {
            if (minimum >= 0)
                return MinimumLink(context, start, end, minimum);
            else
                return MinimumLink(context, end, start, -minimum);
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
        /// Gets or sets the weight of the minimum constraint.
        /// </summary>
        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="MinimumConstraint"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="lowest">The lowest node.</param>
        /// <param name="highest">The highest node.</param>
        /// <param name="minimum">The minimum between the two values.</param>
        /// <exception cref="ArgumentNullException">Thrown if any node name is <c>null</c>.</exception>
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
        public bool Reset(IResetContext context) => true;

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context) => PresenceResult.Success;

        /// <inheritdoc />
        public bool DiscoverNodeRelationships(IRelationshipContext context)
        {
            switch (context.Mode)
            {
                case NodeRelationMode.Links:
                    var lowest = context.Offsets[Lowest];
                    var highest = context.Offsets[Highest];
                    MinimumLink(context, lowest, highest, Minimum);
                    break;
            }
            return true;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            var lowest = context.Relationships.Offsets[Lowest];
            var highest = context.Relationships.Offsets[Highest];
            if (lowest.Representative != highest.Representative)
            {
                context.Circuit.Add(
                new Constraints.MinimumConstraints.MinimumConstraint(
                    Name,
                    highest.Representative, lowest.Representative,
                    lowest.Offset - highest.Offset + Minimum).SetParameter("w", Weight));
                // AddMinimum(context.Circuit, Name, lowest, highest, Minimum, Weight);
            }
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
        }
    }
}
