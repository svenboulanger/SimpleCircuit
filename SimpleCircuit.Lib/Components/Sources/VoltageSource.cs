using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [Drawable("V", "A voltage source.", "Sources")]
    public class VoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            private const string _ac = "ac";
            private const string _pulse = "pulse";
            private const string _square = "square";
            private const string _tri = "tri";
            private const string _step = "step";
            private const string _programmable = "programmable";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "vs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-6, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(6, 0), new(1, 0)), "p", "pos", "a");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(1, new(4, 0));
                        break;

                    case 0:
                    default:
                        SetPinOffset(0, new(-6, 0));
                        SetPinOffset(1, new(6, 0));
                        break;
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanSource(drawing);
                        break;

                    case 0:
                    default:
                        DrawAmericanSource(drawing);
                        break;
                }
            }

            private void DrawAmericanSource(SvgDrawing drawing)
            {
                _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));

                // Circle
                drawing.Circle(new(0, 0), 6);

                // Waveform / inner graphic
                switch (Variants.Select(_ac, _square, _tri, _pulse, _step))
                {
                    case 0:
                        drawing.BeginTransform(new(new(), drawing.CurrentTransform.Matrix.Inverse));
                        drawing.AC();
                        drawing.EndTransform();
                        break;

                    case 1:
                        drawing.BeginTransform(new(new(), drawing.CurrentTransform.Matrix.Inverse));
                        drawing.Polyline(new Vector2[]
                        {
                            new(-3, 0), new(-3, 3), new(0, 3), new(0, -3), new(3, -3), new(3, 0)
                        });
                        drawing.EndTransform();
                        break;

                    case 2:
                        drawing.BeginTransform(new(new(), drawing.CurrentTransform.Matrix.Inverse));
                        drawing.Polyline(new Vector2[]
                        {
                            new(-3, 0), new(-1.5, 1.5), new(1.5, -1.5), new(3, 0)
                        });
                        drawing.EndTransform();
                        break;

                    case 3:
                        drawing.BeginTransform(new(new(), drawing.CurrentTransform.Matrix.Inverse));
                        drawing.Polyline(new Vector2[]
                        {
                            new(-3, 3), new(-1, 3), new(-1, -3), new(1, -3), new(1, 3), new(3, 3)
                        });
                        drawing.EndTransform();
                        break;

                    case 4:
                        drawing.BeginTransform(new(new(), drawing.CurrentTransform.Matrix.Inverse));
                        drawing.Polyline(new Vector2[]
                        {
                            new(-3, 3), new(-1.5, 3), new(-1.5, -3), new(3, -3)
                        });
                        drawing.EndTransform();
                        break;

                    default:
                        drawing.Signs(new(3, 0), new(-3, 0), vertical: true);
                        break;
                }

                if (Variants.Contains(_programmable))
                {
                    drawing.Arrow(new(-6, -6), new(7.5, 7.5), new("arrow", "programmable"));
                    if (_anchors[0].Location.Y > -7)
                        _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    if (_anchors[1].Location.Y < 8.5)
                        _anchors[1] = new LabelAnchorPoint(new(0, 8.5), new(0, 1));
                }
                _anchors.Draw(drawing, this);
            }

            private void DrawEuropeanSource(SvgDrawing drawing)
            {
                drawing.Circle(new(0, 0), 4);
                drawing.Line(new(-4, 0), new(4, 0));

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));

                if (Variants.Contains(_programmable))
                {
                    drawing.Arrow(new(-4, -4), new(6, 6), new("arrow", "programmable"));
                    if (_anchors[0].Location.Y > -5)
                        _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                    if (_anchors[1].Location.Y < 7)
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                }
                _anchors.Draw(drawing, this);
            }
        }
    }
}