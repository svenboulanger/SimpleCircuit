using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
    public class PrepareContext(GraphicalCircuit circuit, ITextFormatter formatter, IStyle style, IDiagnosticHandler diagnostics) : IPrepareContext
    {
        private readonly GraphicalCircuit _circuit = circuit;

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public DesperatenessLevel Desparateness { get; set; } = DesperatenessLevel.Normal;

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

        /// <inheritdoc />
        public ITextFormatter TextFormatter { get; } = formatter ?? throw new ArgumentNullException(nameof(formatter));

        /// <inheritdoc />
        public NodeOffsetGrouper Offsets { get; } = new();

        /// <inheritdoc />
        public NodeGrouper Groups { get; } = new();

        /// <summary>
        /// Gets all drawn groups.
        /// </summary>
        public DrawableGrouper DrawnGroups { get; } = new();

        /// <inheritdoc />
        public IStyle Style { get; } = style ?? throw new ArgumentNullException(nameof(style));

        /// <inheritdoc />
        public ICircuitPresence Find(string name)
        {
            if (_circuit is not null && _circuit.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<ICircuitPresence> FindFilter(string filter)
        {
            var regex = new Regex(filter);
            foreach (var item in _circuit)
            {
                if (regex.IsMatch(item.Name))
                    yield return item;
            }
        }

        /// <inheritdoc />
        public void GroupDrawableTo(IDrawable drawable, string x, string y)
        {
            string repX = Offsets.GetRepresentative(x);
            string repY = Offsets.GetRepresentative(y);
            string groupX = Groups.GetRepresentative(repX);
            string groupY = Groups.GetRepresentative(repY);
            DrawnGroups.Group(drawable, groupX, groupY, repX, repY);
        }

        /// <inheritdoc />
        public void Group(string node1, string node2)
        {
            node1 = Offsets.GetRepresentative(node1);
            node2 = Offsets.GetRepresentative(node2);
            Groups.Group(node1, node2, 0);
        }

        /// <inheritdoc />
        public RelativeItem GetOffset(string node)
        {
            Offsets.TryGet(node, out string representative, out double offset);
            return new(representative, offset);
        }
    }
}
