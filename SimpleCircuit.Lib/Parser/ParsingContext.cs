using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.SimpleTexts;
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
        /// Gets the factory for components.
        /// </summary>
        public DrawableFactoryDictionary Factory { get; } = new();

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Currently included files.
        /// </summary>
        public HashSet<string> Included { get; } = [];

        /// <summary>
        /// Gets extra CSS.
        /// </summary>
        public IList<string> ExtraCss { get; } = [];

        /// <summary>
        /// Gets the referenced variables in the current scope.
        /// </summary>
        public HashSet<string> ReferencedVariables { get; } = [];

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, the assembly should be searched for components using reflection.</param>
        /// <param name="formatter">The text formatter used for the graphical circuit.</param>
        public ParsingContext(bool loadAssembly = true, ITextFormatter formatter = null)
        {
            if (loadAssembly)
            {
                Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
            }
            Circuit = new GraphicalCircuit(formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer()));
        }
    }
}
