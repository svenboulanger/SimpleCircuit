﻿using SpiceSharp.Attributes;
using SpiceSharp.Components;

namespace SimpleCircuit.Components.Constraints.MinimumConstraints
{
    /// <summary>
    /// A component that guarantees a minimum between two coordinates.
    /// </summary>
    [Pin(0, "x"), Pin(1, "tx"), Connected(0, 1), AutoGeneratedBehaviors]
    public partial class MinimumConstraint : Component<Parameters>
    {
        /// <summary>
        /// Creates a new <see cref="MinimumConstraint"/>.
        /// </summary>
        /// <param name="name">The name of the constraint.</param>
        /// <param name="x">The smallest coordinate.</param>
        /// <param name="tx">The largest coordinate.</param>
        /// <param name="minimum">The minimum between the two coordinates.</param>
        public MinimumConstraint(string name, string tx, string x, double offset, double minimum)
            : base(name, 2)
        {
            Connect(tx, x);
            Parameters.Offset = offset;
            Parameters.Minimum = minimum;
        }

        /// <inheritdoc />
        public override string ToString() => $"Minimum {Nodes[0]} -> {Nodes[1]}";
    }
}