﻿using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;

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
        public DesperatenessLevel Desparateness { get; set; }

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

        /// <inheritdoc />
        public ITextFormatter Formatter { get; }

        /// <summary>
        /// Creates a new <see cref="PrepareContext"/>.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        public PrepareContext(GraphicalCircuit circuit, ITextFormatter formatter, IDiagnosticHandler diagnostics)
        {
            _circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));
            Diagnostics = diagnostics;
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            Desparateness = DesperatenessLevel.Normal;
        }

        /// <inheritdoc />
        public ICircuitPresence Find(string name)
        {
            if (_circuit.TryGetValue(name, out var result))
                return result;
            return null;
        }
    }
}