using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of the <see cref="IPrepareContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="PrepareContext"/>.
    /// </remarks>
    /// <param name="circuit">The circuit.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    public class PrepareContext(GraphicalCircuit circuit, IDiagnosticHandler diagnostics) : IPrepareContext
    {
        private readonly GraphicalCircuit _circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public DesperatenessLevel Desparateness { get; set; } = DesperatenessLevel.Normal;

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

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
    }
}
