using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// The context used for collecting information about nodes in a
    /// graphical circuit.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="NodeContext"/>.
    /// </remarks>
    /// <param name="diagnostics">The diagnostics handler.</param>
    public class NodeContext(IDiagnosticHandler diagnostics) : IRelationshipContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public NodeRelationMode Mode { get; set; }

        /// <inheritdoc />
        public NodeOffsetFinder Offsets { get; } = new();

        /// <inheritdoc />
        public Extremes Extremes { get; } = new();

        /// <inheritdoc />
        public HashSet<XYNode> XYSets { get; } = [];

        /// <inheritdoc />
        public void Link(string x, string y)
        {
            string repX = Extremes.Linked[Offsets[x].Representative];
            string repY = Extremes.Linked[Offsets[y].Representative];
            XYSets.Add(new(repX, repY));
        }
    }
}
