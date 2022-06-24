using SimpleCircuit.Diagnostics;
using SpiceSharp.Components;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// A pin without orientation, at a fixed position.
    /// </summary>
    public class FixedPin : Pin
    {
        private readonly ILocatedPresence _origin;

        /// <summary>
        /// Gets or sets the local offset of the pin. This does not include any modifications by the 
        /// </summary>
        public Vector2 Offset { get; set; }

        public FixedPin(string name, string description, ILocatedDrawable owner, Vector2 offset)
            : this(name, description, owner, owner, offset)
        {
        }

        public FixedPin(string name, string description, ILocatedDrawable owner, ILocatedPresence origin, Vector2 offset)
            : base(name, description, owner)
        {
            Offset = offset;
            _origin = origin;
        }

        /// <inheritdoc />
        public override void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
        {
            Vector2 offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            if (offset.X.IsZero())
                context.Shorts.Group(X, _origin.X);
            if (offset.Y.IsZero())
                context.Shorts.Group(Y, _origin.Y);
        }

        /// <inheritdoc />
        public override void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
        {
            Vector2 offset = _origin is ITransformingDrawable tfd ? tfd.TransformOffset(Offset) : Offset;
            var map = context.Nodes.Shorts;
            string x = map[X];
            string ox = map[_origin.X];
            string y = map[Y];
            string oy = map[_origin.Y];
            if (x != ox)
            {
                string i = $"{X}.i";
                context.Circuit.Add(new Resistor($"R{X}", i, x, 1e-3));
                context.Circuit.Add(new VoltageSource($"V{X}", i, ox, offset.X));
            }
            if (y != oy)
            {
                string i = $"{Y}.i";
                context.Circuit.Add(new Resistor($"R{Y}", i, y, 1e-3));
                context.Circuit.Add(new VoltageSource($"V{Y}", y, oy, offset.Y));
            }
        }
    }
}
