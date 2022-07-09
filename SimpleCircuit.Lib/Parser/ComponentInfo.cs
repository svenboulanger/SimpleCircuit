using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents component information.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Gets the token that describes the component name.
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// Gets the full name of the component.
        /// </summary>
        public string Fullname { get; }

        /// <summary>
        /// Gets the label of the component.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets the variants of the component.
        /// </summary>
        public List<VariantInfo> Variants { get; } = new();

        /// <summary>
        /// Gets the component if it has been created.
        /// </summary>
        public IDrawable Component { get; private set; }

        /// <summary>
        /// Creates a new component information.
        /// </summary>
        /// <param name="name">The token name.</param>
        /// <param name="fullname">The full name of the component.</param>
        public ComponentInfo(Token name, string fullname)
        {
            Name = name;
            Fullname = fullname;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component, or <c>null</c> if no component could be created.</returns>
        public IDrawable GetOrCreate(ParsingContext context)
        {
            if (Component == null)
            {
                Component = context.GetOrCreate(Fullname, context.Options, context.Diagnostics);
                if (Component == null)
                {
                    context.Diagnostics?.Post(Name, ErrorCodes.CouldNotRecognizeOrCreateComponent, Fullname);
                    return null;
                }

                // Handle the label
                if (Label != null && Component is ILabeled labeled)
                    labeled.Label = Label;

                // Handle variants
                foreach (var variant in Variants)
                {
                    if (variant.Include)
                        Component.Variants.Add(variant.Name);
                    else
                        Component.Variants.Remove(variant.Name);
                }
            }
            return Component;
        }
    }
}
