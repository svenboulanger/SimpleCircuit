using SimpleCircuit.Circuits.Contexts;
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
        private IDrawable _component;

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
        public List<Token> Labels { get; } = new(2);

        /// <summary>
        /// Gets the variants of the component.
        /// </summary>
        public List<VariantInfo> Variants { get; } = new();

        /// <summary>
        /// Gets the component if it has been created.
        /// </summary>
        public IDrawable Component => _component;

        /// <summary>
        /// Creates a new component information.
        /// </summary>
        /// <param name="name">The token name.</param>
        /// <param name="fullname">The full name of the component.</param>
        public ComponentInfo(Token name, string fullname)
        {
            Name = name;
            Fullname = fullname;
            _component = null;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component, or <c>null</c> if no component could be found or created.</returns>
        public IDrawable GetOrCreate(ParsingContext context)
        {
            if (_component == null)
            {
                _component = context.GetOrCreate(Fullname, context.Options, context.Diagnostics);
                if (_component == null)
                {
                    context.Diagnostics?.Post(Name, ErrorCodes.CouldNotRecognizeOrCreateComponent, Fullname);
                    return null;
                }

                // Handle the label
                if (Labels.Count > 0 && _component is ILabeled labeled)
                {
                    if (Labels.Count > labeled.Labels.Maximum)
                    {
                        context.Diagnostics?.Post(Labels[labeled.Labels.Count], ErrorCodes.TooManyLabels);
                        for (int i = 0; i < labeled.Labels.Maximum; i++)
                            labeled.Labels[i] = Labels[i].Content[1..^1].ToString();
                    }
                    else
                    {
                        for (int i = 0; i < Labels.Count; i++)
                            labeled.Labels[i] = Labels[i].Content[1..^1].ToString();
                    }    
                }
                
                // Handle variants
                foreach (var variant in Variants)
                {
                    if (variant.Include)
                        _component.Variants.Add(variant.Name);
                    else
                        _component.Variants.Remove(variant.Name);
                }
            }
            return _component;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component; or <c>null</c> if no component could be found.</returns>
        public IDrawable Get(IPrepareContext context)
        {
            if (_component == null)
            {
                _component = context.Find(Fullname) as IDrawable;
                if (_component == null)
                    context.Diagnostics?.Post(Name, ErrorCodes.CouldNotFindDrawable, Name.Content);
            }
            return _component;
        }

        /// <inheritdoc />
        public override string ToString() => Fullname;
    }
}
