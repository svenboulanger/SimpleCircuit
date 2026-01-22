using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Parser;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components;

/// <summary>
/// A constraint between two nodes that tries to guarantee a minimum between the two.
/// </summary>
public class MinimumConstraint : ICircuitSolverPresence
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public List<TextLocation> Sources { get; } = [];

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
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        Lowest = !string.IsNullOrWhiteSpace(lowest) ? lowest : throw new ArgumentNullException(nameof(lowest)); ;
        Highest = !string.IsNullOrWhiteSpace(highest) ? highest : throw new ArgumentNullException(nameof(highest)); ;
        Minimum = minimum;
    }

    /// <inheritdoc />
    public PresenceResult Prepare(IPrepareContext context)
    {
        if (context.Mode == PreparationMode.Offsets)
            context.Group(Lowest, Highest);
        return PresenceResult.Success;
    }

    /// <inheritdoc />
    public void Register(IRegisterContext context)
    {
        var lowest = context.GetOffset(Lowest);
        var highest = context.GetOffset(Highest);
        if (lowest.Representative != highest.Representative)
            AddMinimum(context.Circuit, Name, lowest, highest, Minimum, Weight);
    }

    /// <inheritdoc />
    public void Update(IUpdateContext context)
    {
    }

    /// <summary>
    /// Adds a structure to the circuit that tries to guarantee a minimum between two relative items.
    /// </summary>
    /// <param name="circuit">The circuit.</param>
    /// <param name="name">A unique name for the structure.</param>
    /// <param name="lowest">The lowest node.</param>
    /// <param name="highest">The highest node.</param>
    /// <param name="minimum">The offset between the two nodes.</param>
    /// <param name="weight">The weight for the constraint when extended beyond the minimum.</param>
    public static void AddMinimum(IEntityCollection circuit, string name, RelativeItem lowest, RelativeItem highest, double minimum, double weight = 1.0)
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
        if (StringComparer.Ordinal.Equals(start.Representative, end.Representative))
        {
            // Double check that the minimum is guaranteed
            if (start.Offset + minimum < end.Offset - 1e-2)
            {
                throw new ArgumentException("Invalid minimum");
            }
        }
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
        var offset = new Vector2(fromX.Offset - toX.Offset, fromY.Offset - toY.Offset);
        var component = new Constraints.SlopedMinimumConstraints.SlopedMinimumConstraint(
            name, fromX.Representative, fromY.Representative, toX.Representative, toY.Representative, offset, normal, minimum);
        component.SetParameter("weight", weight);
        circuit.Add(component);
    }
}
