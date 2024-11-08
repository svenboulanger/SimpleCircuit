﻿using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A context used for preparing circuit presences.
    /// </summary>
    public interface IPrepareContext
    {
        /// <summary>
        /// Gets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// The current mode of preparation.
        /// </summary>
        /// <remarks>
        /// This mode allows doing partial preparations.
        /// </remarks>
        public DesperatenessLevel Desparateness { get; }

        /// <summary>
        /// Gets the mode of preparation.
        /// </summary>
        public PreparationMode Mode { get; }

        /// <summary>
        /// Finds a circuit presence with the given name.
        /// </summary>
        /// <param name="name">The full name of the presence.</param>
        /// <returns>The circuit presence, of <c>null</c> if it doesn't exist.</returns>
        public ICircuitPresence Find(string name);
    }
}
