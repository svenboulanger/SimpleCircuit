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

        /// <inheritdoc />
        protected override Transform CreateTransform() => new(Location, Matrix2.Identity);

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
        public override void Reset()
        {
            base.Reset();
            Location = new();
        }

        /// <inheritdoc />
        public override void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            double x = 0, y = 0;

            // Let's first deal with our own problems
            if (state.TryGetValue(context.Nodes.Shorts[X], out var solX))
                x = solX.Value;
            if (state.TryGetValue(context.Nodes.Shorts[Y], out var solY))
                y = solY.Value;
            Location = new Vector2(x, y);

            // Give the pins a chance to update as well
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Update(state, context, diagnostics);
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            base.DiscoverNodeRelationships(context, diagnostics);
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].DiscoverNodeRelationships(context, diagnostics);

            switch (context.Mode)
            {
                case NodeRelationMode.Groups:
                    string x = context.Extremes.Linked[context.Shorts[X]];
                    string y = context.Extremes.Linked[context.Shorts[Y]];
                    context.XYSets.Add(new XYNode(x, y));
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Register(context, diagnostics);
        }
    }
}
