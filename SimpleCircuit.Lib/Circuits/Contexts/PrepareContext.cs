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
        private readonly Dictionary<string, Dictionary<string, HashSet<IDrawable>>> _linkedGroups = [];

        /// <summary>
        /// Gets the grouped Y-coordinates.
        /// </summary>
        public IEnumerable<string> DrawableGroupY => _linkedGroups.Keys;

        /// <inheritdoc />
        public int DrawableGroupCount { get; private set; } = 0;

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
            x = Groups[Offsets[x].Representative];
            y = Groups[Offsets[y].Representative];
            if (!_linkedGroups.TryGetValue(y, out var dictY))
            {
                dictY = [];
                _linkedGroups.Add(y, dictY);
            }
            if (!dictY.TryGetValue(x, out var setX))
            {
                setX = [];
                dictY.Add(x, setX);
                DrawableGroupCount++;
            }
            setX.Add(drawable);
        }

        /// <summary>
        /// Returns, for a given Y-coordinate group, the different X-coordinate groups.
        /// </summary>
        /// <param name="y">The Y-coordinate group key.</param>
        /// <returns>Returns the coordinate groups.</returns>
        public IEnumerable<KeyValuePair<string, IEnumerable<IDrawable>>> GetDrawableGroups(string y)
        {
            if (_linkedGroups.TryGetValue(y, out var results))
            {
                foreach (var pair in results)
                    yield return new(pair.Key, pair.Value);
            }
        }
    }
}
