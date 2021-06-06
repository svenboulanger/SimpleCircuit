using SimpleCircuit.Components;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A parsing context.
    /// </summary>
    public class ParsingContext
    {
        /// <summary>
        /// Gets the factory for components.
        /// </summary>
        public ComponentFactory Factory { get; } = new ComponentFactory();

        /// <summary>
        /// Gets the subcircuit definitions.
        /// </summary>
        public KeySearch<SubcircuitDefinition> Definitions { get; } = new KeySearch<SubcircuitDefinition>();

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public Circuit Circuit { get; } = new Circuit();

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <returns>The component.</returns>
        public IComponent GetOrCreate(string name)
        {
            IComponent component;

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
                else if (Circuit.TryGetValue(name, out component))
                    return component;

                // We didn't find it, so let's create it!
                component = new Subcircuit(name, definition.Definition, definition.Ports);
                Circuit.Add(component);
                return component;
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
            else if (Circuit.TryGetValue(name, out component))
                return component;

            // Didn't find the component, let's create it!
            component = Factory.Create(name);
            Circuit.Add(component);
            return component;
        }
    }
}
