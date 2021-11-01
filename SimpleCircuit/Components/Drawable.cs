using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System;

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

        /// <inheritdoc />
        public virtual void Render(SvgDrawing drawing)
        {
            drawing.StartGroup(Name, GetType().Name.ToLower());
            Draw(drawing);
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
