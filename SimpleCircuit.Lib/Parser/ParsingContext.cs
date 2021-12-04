using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;

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
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        public ParsingContext()
        {
            Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="options">Options that can be used for the component.</param>
        /// <returns>The component.</returns>
        public IDrawable GetOrCreate(string name, Options options)
        {
            IDrawable result;
            if (Circuit.TryGetValue(name, out var presence) && presence is IDrawable drawable)
                return drawable;
            result = Factory.Create(name, options);
            Circuit.Add(result);
            return result;
        }
    }
}
