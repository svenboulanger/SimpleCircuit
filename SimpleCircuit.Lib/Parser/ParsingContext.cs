using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System;
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
        /// Gets or sets the number of wires.
        /// </summary>
        public int WireCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of virtual coordinates.
        /// </summary>
        public int VirtualCoordinateCount { get; set; } = 0;

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
        public GraphicalCircuit Circuit { get; } = new GraphicalCircuit();

        /// <summary>
        /// Gets the stack of current sections. This can be used to separate parts of the circuit
        /// from each other.
        /// </summary>
        public Stack<string> Section { get; } = new Stack<string>();

        /// <summary>
        /// Gets the defined sections until now.
        /// </summary>
        public Dictionary<string, Token> SectionTemplates { get; } = new();

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        public ParsingContext()
        {
            Factory.RegisterAssembly(typeof(ParsingContext).Assembly);

            // Link the circuit options to the actual circuit
            Options.SpacingXChanged += (sender, args) => Circuit.SpacingX = Options.SpacingX;
            Options.SpacingYChanged += (sender, args) => Circuit.SpacingY = Options.SpacingY;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="fullname">The full name of the drawable.</param>
        /// <param name="options">Options that can be used for the component.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>The component, or <c>null</c> if no drawable could be created.</returns>
        public IDrawable GetOrCreate(string fullname, Options options, IDiagnosticHandler diagnostics)
        {
            IDrawable result;
            if (Circuit.TryGetValue(fullname, out var presence) && presence is IDrawable drawable)
                return drawable;
            result = Factory.Create(fullname, options, diagnostics);
            if (result != null)
                Circuit.Add(result);
            return result;
        }
    }
}
