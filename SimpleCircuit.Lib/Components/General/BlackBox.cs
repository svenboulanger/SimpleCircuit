using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SpiceSharp.Simulations;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A generic black box.
    /// </summary>
    [Drawable("BB", "A black box. Pins are created on the fly, where the first character (n, s, e, w for north, south, east and west respectively) indicates the side of the pin.", "General")]
    public partial class BlackBox : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
        {
            var instance = (Instance)base.Create(key, name, options, diagnostics);
            if (options != null)
            {
                instance.MinSpaceX = options.HorizontalPinSpacing;
                instance.MinSpaceY = options.VerticalPinSpacing;
            }
            return instance;
        }

        protected class Instance : ILocatedDrawable, ILabeled
        {
            private readonly PinCollection _pins;

            /// <inheritdoc />
            public VariantSet Variants { get; } = new();

            /// <inheritdoc />
            public int Order => 0;

            /// <inheritdoc />
            IPinCollection IDrawable.Pins => _pins;

            /// <inheritdoc />
            public string X { get; }

            /// <inheritdoc />
            public string Y { get; }

            /// <inheritdoc />
            public Vector2 Location { get; protected set; }

            /// <inheritdoc />
            public Vector2 EndLocation { get; protected set; }

            /// <inheritdoc />
            public string Name { get; }

            [Description("The minimum horizontal space between two pins.")]
            public double MinSpaceX { get => _pins.MinSpaceX; set => _pins.MinSpaceX = value; }

            [Description("The minimum vertical space between two pins.")]
            public double MinSpaceY { get => _pins.MinSpaceY; set => _pins.MinSpaceY = value; }

            [Description("The minimum horizontal space between a pin and the edge of the black box.")]
            public double MinEdgeX { get => _pins.MinEdgeX; set => _pins.MinEdgeX = value; }

            [Description("The minimum vertical space between a pin and the edge of the black box.")]
            public double MinEdgeY { get => _pins.MinEdgeY; set => _pins.MinEdgeY = value; }

            [Description("The minimum width.")]
            public double MinWidth { get => _pins.MinWidth; set => _pins.MinWidth = value; }

            [Description("The minimum height.")]
            public double MinHeight { get => _pins.MinHeight; set => _pins.MinHeight = value; }

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Creates a new black box.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));
                Name = name;
                _pins = new(this);
                X = $"{Name}.x";
                Y = $"{Name}.y";
                EndLocation = new(20, 20);
            }

            /// <inheritdoc />
            public void Reset() { }

            /// <inheritdoc />
            public PresenceResult Prepare(GraphicalCircuit circuit, PresenceMode mode, IDiagnosticHandler diagnostics)
            {
                return PresenceResult.Success;
            }

            /// <inheritdoc />
            public void Render(SvgDrawing drawing)
            {
                var go = new GraphicOptions(GetType().Name.ToLower()) { Id = Name };
                go.Classes.Add("blackbox");
                drawing.StartGroup(go);
                var center = 0.5 * (Location + EndLocation);
                drawing.Rectangle(EndLocation - Location, center);

                // Draw the label
                drawing.Text(Labels[0], center, new());

                // Draw the port names
                _pins.Render(drawing);
                drawing.EndGroup();
            }

            /// <inheritdoc />
            public void DiscoverNodeRelationships(NodeContext context, IDiagnosticHandler diagnostics)
            {
                _pins.DiscoverNodeRelationships(context);

                switch (context.Mode)
                {
                    case NodeRelationMode.Groups:
                        context.Link(X, Y);
                        break;

                    default:
                        break;
                }
            }

            /// <inheritdoc />
            public void Register(CircuitSolverContext context, IDiagnosticHandler diagnostics)
            {
                _pins.Register(context);
            }

            /// <inheritdoc />
            public void Update(IBiasingSimulationState state, CircuitSolverContext context, IDiagnosticHandler diagnostics)
            {
                Location = context.Nodes.GetValue(state, X, Y);
                EndLocation = context.Nodes.GetValue(state, _pins.Right, _pins.Bottom);

                // Update all pin locations as well
                // We ignore pin 0, because that is a dummy pin
                for (int i = 1; i < _pins.Count; i++)
                    _pins[i].Update(state, context, diagnostics);
            }
        }
    }
}