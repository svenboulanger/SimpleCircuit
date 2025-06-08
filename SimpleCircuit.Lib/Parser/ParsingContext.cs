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
        /// Gets the options.
        /// </summary>
        public Options Options { get; } = new();

        /// <summary>
        /// Gets or sets whether the parser should try to be compatible with SimpleCircuit 2.x.
        /// </summary>
        public bool CompatibilityMode { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag that allows definitions of subcircuit definitions.
        /// </summary>
        public bool AllowSubcircuitDefinitions { get; set; } = true;

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Currently included files.
        /// </summary>
        public HashSet<string> Included { get; } = [];

        /// <summary>
        /// Gets the referenced variables in the current scope.
        /// </summary>
        public HashSet<string> ReferencedVariables { get; } = [];
    }
}
