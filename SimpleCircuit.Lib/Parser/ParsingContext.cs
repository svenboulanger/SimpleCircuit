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
        public ComponentFactory Factory { get; } = new ComponentFactory();

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets the subcircuit definitions.
        /// </summary>
        public KeySearch<SubcircuitDefinition> Definitions { get; } = new KeySearch<SubcircuitDefinition>();

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public GraphicalCircuit Circuit { get; } = new GraphicalCircuit();

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="options">Options that can be used for the component.</param>
        /// <returns>The component.</returns>
        public IDrawable GetOrCreate(string name, Options options)
        {
            IDrawable result;

            // First try to find a subcircuit
            var exact = Definitions.Search(name, out var definition);
            if (definition != null)
            {
                if (exact)
                {
                    int index = 1;
                    string newName = $"{name}:1";
                    while (Circuit.ContainsKey(newName))
                        newName = $"{name}:{++index}";
                    name = newName;
                }
                else if (Circuit.TryGetValue(name, out var presence) && presence is IDrawable drawable)
                    return drawable;

                // We didn't find it, so let's create it!
                result = new Subcircuit(name, definition.Definition, options, definition.Ports);
                Circuit.Add(result);
                return result;
            }

            // We didn't find a subcircuit definition, so check regular components
            if (Factory.IsExact(name))
            {
                int index = 1;
                string newName = $"{name}:1";
                while (Circuit.ContainsKey(newName))
                    newName = $"{name}:{++index}";
                name = newName;
            }
            else if (Circuit.TryGetValue(name, out var presence) && presence is IDrawable drawable)
                return drawable;

            // Didn't find the component, let's create it!
            result = Factory.Create(name, options);
            Circuit.Add(result);
            return result;
        }
    }
}
