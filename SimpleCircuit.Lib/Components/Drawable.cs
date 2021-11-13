using SimpleCircuit.Components.Pins;
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
        /// <inheritdoc />
        public string Name { get; }

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

        /// <summary>
        /// Draw the component.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void Draw(SvgDrawing drawing);

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
            go.Classes.Add(GetType().Name.ToLower());
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
        public abstract void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public abstract void Register(CircuitContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public abstract void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics);
    }
}
