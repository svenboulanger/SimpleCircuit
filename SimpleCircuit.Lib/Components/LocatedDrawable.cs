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
        public override bool Reset(IDiagnosticHandler diagnostics)
        {
            if (!base.Reset(diagnostics))
                return false;
            Location = new();
            return true;
        }

        /// <inheritdoc />
        public override void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            Location = context.Nodes.GetValue(state, X, Y);

            // Give the pins a chance to update as well
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Update(state, context, diagnostics);
        }

        /// <inheritdoc />
        public override bool DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            if (!base.DiscoverNodeRelationships(context, diagnostics))
                return false;
            for (int i = 0; i < Pins.Count; i++)
            {
                if (!Pins[i].DiscoverNodeRelationships(context, diagnostics))
                    return false;
            }

            switch (context.Mode)
            {
                case NodeRelationMode.Groups:
                    context.Link(X, Y);
                    break;
            }
            return true;
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            for (int i = 0; i < Pins.Count; i++)
                Pins[i].Register(context, diagnostics);
        }
    }
}
