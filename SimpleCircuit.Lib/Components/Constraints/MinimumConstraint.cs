using SimpleCircuit.Circuits.Contexts;
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
            var component = new Constraints.MinimumConstraints.MinimumConstraint(name, highest.Representative, lowest.Representative, delta, minimum);
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
        public static void AddDirectionalMinimum(IEntityCollection circuit, string name, RelativeItem start, RelativeItem end, double minimum, double weight = 1)
        {
            if (minimum > 0)
                AddMinimum(circuit, name, start, end, minimum, weight);
            else
                AddMinimum(circuit, name, end, start, -minimum, weight);
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
        public PresenceResult Prepare(IPrepareContext context)
        {
            if (context.Mode == PreparationMode.Offsets)
            {
                var a = context.Offsets[Lowest];
                var b = context.Offsets[Highest];
                context.Groups.Group(a.Representative, b.Representative);
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
            var lowest = context.Relationships.Offsets[Lowest];
            var highest = context.Relationships.Offsets[Highest];
            if (lowest.Representative != highest.Representative)
                AddMinimum(context.Circuit, Name, lowest, highest, Minimum, Weight);
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context)
        {
        }
    }
}
