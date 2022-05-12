using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component.
    /// </summary>
    public abstract class Drawable : IDrawable
    {
        private readonly HashSet<string> _variants = new(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the type name of the instance.
        /// </summary>
        public virtual string Type => "Drawable";

        /// <summary>
        /// Gets the pins of the component.
        /// </summary>
        public PinCollection Pins { get; } = new();

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => Pins;

        /// <summary>
        /// Resolves how pins are updated before solving.
        /// </summary>
        protected IVariantResolver PinUpdate { get; set; }

        /// <summary>
        /// Resolves how the drawable is drawn.
        /// </summary>
        protected IVariantResolver DrawingVariants { get; set; }

        /// <inheritdoc />
        public virtual int Order => 0;

        /// <summary>
        /// Allows adding classes for the group node that groups all drawing elements.
        /// </summary>
        protected virtual IEnumerable<string> GroupClasses { get; }

        /// <summary>
        /// Creates a new component.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        protected Drawable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        /// <inheritdoc />
        public virtual void AddVariant(string variant)
            => _variants.Add(variant);

        /// <inheritdoc />
        public virtual void RemoveVariant(string variant)
            => _variants.Remove(variant);

        /// <inheritdoc />
        public virtual void CollectPossibleVariants(ISet<string> variants)
        {
            PinUpdate?.CollectPossibleVariants(variants);
            DrawingVariants?.CollectPossibleVariants(variants);
        }

        /// <summary>
        /// Creates a transform.
        /// </summary>
        /// <returns></returns>
        protected virtual Transform CreateTransform() => Transform.Identity;

        /// <inheritdoc />
        public virtual void Render(SvgDrawing drawing)
        {
            // Group all elements
            var go = new GraphicOptions() { Id = Name };
            if (!string.IsNullOrWhiteSpace(Type))
                go.Classes.Add(Type.ToLower());
            foreach (string name in _variants)
                go.Classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    go.Classes.Add(name);
            }
            drawing.StartGroup(go);

            // Transform all the elements inside the drawing method
            drawing.BeginTransform(CreateTransform());
            if (DrawingVariants != null)
            {
                var context = new VariantResolverContext<SvgDrawing>(_variants, drawing);
                DrawingVariants.Resolve(context);
            }
            drawing.EndTransform();

            // Stop grouping elements
            drawing.EndGroup();
        }

        /// <inheritdoc />
        public virtual void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            var c = new VariantResolverContext(_variants);
            PinUpdate?.Resolve(c);
        }

        /// <inheritdoc />
        public abstract void Register(CircuitContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public abstract void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Converts the drawable to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Name;
    }
}
