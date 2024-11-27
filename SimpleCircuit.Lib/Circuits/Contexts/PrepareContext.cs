using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of the <see cref="IPrepareContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="PrepareContext"/>.
    /// </remarks>
    /// <param name="circuit">The circuit.</param>
    /// <param name="formatter">The text formatter.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    public class PrepareContext(GraphicalCircuit circuit, ITextFormatter formatter, IDiagnosticHandler diagnostics) : IPrepareContext
    {
        private readonly GraphicalCircuit _circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public DesperatenessLevel Desparateness { get; set; } = DesperatenessLevel.Normal;

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

        /// <inheritdoc />
        public ITextFormatter TextFormatter { get; } = formatter ?? throw new ArgumentNullException(nameof(formatter));

        /// <inheritdoc />
        public NodeOffsetFinder Offsets { get; } = new();

        /// <inheritdoc />
        public NodeGrouper Groups { get; } = new();

        /// <summary>
        /// Gets all drawn groups.
        /// </summary>
        public DrawableGrouper DrawnGroups { get; } = new();

        /// <inheritdoc />
        public ICircuitPresence Find(string name)
        {
            if (_circuit.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <inheritdoc />
        public void GroupDrawableTo(IDrawable drawable, string x, string y)
        {
            string repX = Offsets[x].Representative;
            string repY = Offsets[y].Representative;
            string groupX = Groups.GetRepresentative(repX);
            string groupY = Groups.GetRepresentative(repY);
            DrawnGroups.Group(drawable, groupX, groupY, repX, repY);
        }

        /// <inheritdoc />
        public void Group(string node1, string node2)
        {
            node1 = Offsets[node1].Representative;
            node2 = Offsets[node2].Representative;
            Groups.Group(node1, node2, 0);
        }
    }
}
