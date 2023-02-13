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
        /// <summary>
        /// Gets the variants of the drawable.
        /// </summary>
        public VariantSet Variants { get; } = new();

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
        public virtual void Reset()
        {
            foreach (var pin in Pins)
                pin.Reset();
        }

        /// <inheritdoc />
        public virtual PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
        {
            return PresenceResult.Success;
        }

        /// <summary>
        /// Creates a transform.
        /// </summary>
        /// <returns></returns>
        protected virtual Transform CreateTransform() => Transform.Identity;

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void Draw(SvgDrawing drawing);

        /// <inheritdoc />
        public virtual void Render(SvgDrawing drawing)
        {
            // Group all elements
            var go = new GraphicOptions() { Id = Name };
            if (!string.IsNullOrWhiteSpace(Type))
                go.Classes.Add(Type.ToLower());
            foreach (string name in Variants)
                go.Classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    go.Classes.Add(name);
            }
            drawing.StartGroup(go);

            // Transform all the elements inside the drawing method
            drawing.BeginTransform(CreateTransform());
            Draw(drawing);
            drawing.EndTransform();

            // Stop grouping elements
            drawing.EndGroup();
        }

        /// <inheritdoc />
        public virtual bool DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
            => true;

        /// <inheritdoc />
        public abstract void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public abstract void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Converts the drawable to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Name;
    }
}
