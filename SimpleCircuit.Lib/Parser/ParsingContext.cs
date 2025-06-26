using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A parsing context.
    /// </summary>
    public class ParsingContext
    {
        /// <summary>
        /// A flag that determines whether subcircuit definitions are allowed.
        /// </summary>
        public bool AllowSubcircuitDefinitions { get; set; } = true;

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets the referenced variables in the current scope.
        /// </summary>
        public HashSet<string> ReferencedVariables { get; } = [];
    }
}
