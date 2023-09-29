using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

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

            [Description("The offset for the label relative to the center.")]
            public Vector2 Offset { get; set; }

            [Description("The margin of labels to the edge of the black box.")]
            public double LabelMargin { get; set; } = 1.0;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public IEnumerable<string> Properties => Drawable.GetProperties(this);

            /// <inheritdoc />
            public Bounds Bounds { get; private set; }

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
            public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
                => Drawable.SetProperty(this, propertyToken, value, diagnostics);

            /// <inheritdoc />
            public bool Reset(IResetContext context)
            {
                foreach (var pin in _pins)
                {
                    if (!pin.Reset(context))
                        return false;
                }
                return true;
            }

            /// <inheritdoc />
            public PresenceResult Prepare(IPrepareContext context) => PresenceResult.Success;

            /// <inheritdoc />
            public void Render(SvgDrawing drawing)
            {
                var go = new GraphicOptions(GetType().Name.ToLower()) { Id = Name };
                go.Classes.Add("blackbox");
                drawing.BeginGroup(go);
                var center = 0.5 * (Location + EndLocation);
                drawing.Rectangle(EndLocation - Location, center);

                // Draw the label
                Labels.BoxedLabel(Variants, Location, EndLocation, margin: LabelMargin);

                // Draw the port names
                _pins.Render(drawing);
                Bounds = drawing.EndGroup();
            }

            /// <inheritdoc />
            public bool DiscoverNodeRelationships(IRelationshipContext context)
            {
                if (!_pins.DiscoverNodeRelationships(context))
                    return false;

                switch (context.Mode)
                {
                    case NodeRelationMode.Groups:
                        context.Link(X, Y);
                        break;
                }
                return true;
            }

            /// <inheritdoc />
            public void Register(IRegisterContext context)
            {
                _pins.Register(context);
            }

            /// <inheritdoc />
            public void Update(IUpdateContext context)
            {
                Location = context.GetValue(X, Y);
                EndLocation = context.GetValue(_pins.Right, _pins.Bottom);

                // Update all pin locations as well
                // We ignore pin 0, because that is a dummy pin
                for (int i = 1; i < _pins.Count; i++)
                    _pins[i].Update(context);
            }
        }
    }
}