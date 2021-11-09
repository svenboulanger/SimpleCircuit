using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SpiceSharp.Simulations;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A generic implementation of a component.
    /// </summary>
    public abstract class LocatedDrawable : Drawable, ILocatedDrawable
    {
        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatedDrawable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected LocatedDrawable(string name)
            : base(name)
        {            
            X = $"{name}.x";
            Y = $"{name}.y";
        }

        /// <inheritdoc />
        public override void Update(IBiasingSimulationState state, CircuitContext context, IDiagnosticHandler diagnostics)
        {
            double x = 0, y = 0;

            // Let's first deal with our own problems
            if (state.TryGetValue(context.Nodes.Shorts[X], out var solX))
                x = solX.Value;
            else
                diagnostics.Post(new DiagnosticMessage(SeverityLevel.Warning, "UW001", $"Could not find X-coordinate of {Name} in solver."));
            if (state.TryGetValue(context.Nodes.Shorts[Y], out var solY))
                y = solY.Value;
            else
                diagnostics.Post(new DiagnosticMessage(SeverityLevel.Warning, "UW002", $"Could not find Y-coordinate of {Name} in solver."));
            Location = new Vector2(x, y);

            // Give the pins a chance to update as well
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Update(state, context, diagnostics);
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].DiscoverNodeRelationships(context, diagnostics);
        }

        /// <inheritdoc />
        public override void Register(CircuitContext context, IDiagnosticHandler diagnostics)
        {
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Register(context, diagnostics);
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            drawing.StartGroup(Name, GetType().Name.ToLower());
            drawing.BeginTransform(new(Location, Matrix2.Identity));
            Draw(drawing);
            drawing.EndTransform();
            drawing.EndGroup();
        }
    }
}
