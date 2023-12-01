using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Information for components.
    /// </summary>
    /// <remarks>
    /// Creates a new component information.
    /// </remarks>
    /// <param name="name">The token name.</param>
    /// <param name="fullname">The full name of the component.</param>
    public class ComponentInfo(Token name, string fullname) : IDrawableInfo
    {
        private IDrawable _component = null;

        /// <inheritdoc />
        public Token Source { get; } = name;

        /// <inheritdoc />
        public string Fullname { get; } = fullname;

        /// <inheritdoc />
        public IList<Token> Labels { get; } = new List<Token>(2);

        /// <inheritdoc />
        public IList<VariantInfo> Variants { get; } = new List<VariantInfo>();

        /// <inheritdoc />
        public IDictionary<Token, object> Properties { get; } = new Dictionary<Token, object>();

        /// <summary>
        /// Gets the component if it has been created.
        /// </summary>
        public IDrawable Component => _component;

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component, or <c>null</c> if no component could be found or created.</returns>
        public IDrawable GetOrCreate(ParsingContext context)
        {
            if (_component != null)
                return _component;

            if (context.Circuit.TryGetValue(Fullname, out var presence) && presence is IDrawable drawable)
                _component = drawable;
            else
            {
                _component = context.Factory.Create(Fullname, context.Options, context.Diagnostics);
                if (_component != null)
                    context.Circuit.Add(_component);
            }

            if (_component == null)
            {
                context.Diagnostics?.Post(Source, ErrorCodes.CouldNotRecognizeOrCreateComponent, Fullname);
                return null;
            }

            // Handle the label
            if (Labels.Count > 0 && _component is ILabeled labeled)
            {
                for (int i = 0; i < Labels.Count; i++)
                    labeled.Labels[i].Value = Labels[i].Content[1..^1].ToString();
            }

            // Handle variants
            foreach (var variant in Variants)
            {
                if (variant.Include)
                    _component.Variants.Add(variant.Name);
                else
                    _component.Variants.Remove(variant.Name);
            }

            // Handle properties
            foreach (var property in Properties)
                _component.SetProperty(property.Key, property.Value, context.Diagnostics);
            return _component;
        }

        /// <summary>
        /// Gets a component, potentially using a context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component; or <c>null</c> if no component could be found.</returns>
        public IDrawable Get(IPrepareContext context)
        {
            if (_component != null)
                return _component;

            _component = context.Find(Fullname) as IDrawable;
            if (_component == null)
                context.Diagnostics?.Post(Source, ErrorCodes.CouldNotFindDrawable, Fullname);
            return _component;
        }

        /// <inheritdoc />
        public override string ToString() => Fullname;
    }
}
