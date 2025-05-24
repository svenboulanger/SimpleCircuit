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
    public class PrepareContext : IPrepareContext
    {
        private readonly GraphicalCircuit _circuit;

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public DesperatenessLevel Desparateness { get; set; } = DesperatenessLevel.Normal;

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

        /// <inheritdoc />
        public ITextFormatter TextFormatter { get; }

        /// <inheritdoc />
        public NodeOffsetGrouper Offsets { get; } = new();

        /// <inheritdoc />
        public NodeGrouper Groups { get; } = new();

        /// <summary>
        /// Gets all drawn groups.
        /// </summary>
        public DrawableGrouper DrawnGroups { get; } = new();

        /// <inheritdoc />
        public IStyle Style { get; }

        /// <summary>
        /// Creates a new <see cref="PrepareContext"/>.
        /// </summary>
        /// <param name="circuit">The graphical circuit.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public PrepareContext(GraphicalCircuit circuit, IDiagnosticHandler diagnostics)
        {
            _circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));
            Style = circuit.Style;
            TextFormatter = circuit.TextFormatter;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Creates a new <see cref="PrepareContext"/>.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="style">The style.</param>
        /// <param name="diagnostics">the diagnostics handler.</param>
        public PrepareContext(ITextFormatter formatter, IStyle style, IDiagnosticHandler diagnostics)
        {
            _circuit = null;
            Style = style ?? throw new ArgumentNullException(nameof(style));
            TextFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            Diagnostics = diagnostics;

        }

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
