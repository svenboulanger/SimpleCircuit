using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// Default implementation for pins.
    /// </summary>
    public abstract class Pin : IPin
    {
        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public ILocatedDrawable Owner { get; }

        /// <inheritdoc />
        public int Connections { get; set; }

        /// <inheritdoc />
        public string X { get; }

        /// <inheritdoc />
        public string Y { get; }

        /// <inheritdoc />
        public Vector2 Location { get; protected set; }

        protected Pin(string name, string description, ILocatedDrawable owner)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Owner = owner ?? throw new ArgumentNullException(nameof(name));
            X = $"{Owner.Name}[{Name}].x";
            Y = $"{Owner.Name}[{Name}].y";
        }

        /// <inheritdoc />
        public virtual void Reset()
        {
            Location = new();
        }

        /// <inheritdoc />
        public void Prepare(GraphicalCircuit circuit, IDiagnosticHandler diagnostics)
        {
        }

        /// <inheritdoc />
        public abstract void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public abstract void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics);

        /// <inheritdoc />
        public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            double x = 0, y = 0;
            if (state.TryGetValue(context.Nodes.Shorts[X], out var xValue))
                x = xValue.Value;
            if (state.TryGetValue(context.Nodes.Shorts[Y], out var yValue))
                y = yValue.Value;
            Location = new(x, y);
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Owner.Name}[{Name}]";
    }
}
