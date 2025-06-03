using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Nand gate.
    /// </summary>
    [Drawable("NAND", "A NAND gate.", "Digital")]
    public class Nand : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ScaledOrientedDrawable(name), IStandardizedDrawable, IBoxDrawable
        {
            private int _inputs = 2;
            private double _spacing = 5;

            /// <inheritdoc />
            public override string Type => "nand";

            [Description("The number of inputs (1 to 10)")]
            public int Inputs
            {
                get => _inputs;
                set
                {
                    _inputs = value;
                    if (_inputs < 1)
                        _inputs = 1;
                    if (_inputs > 10)
                        _inputs = 10;
                }
            }

            [Description("The space between inputs")]
            public double Spacing
            {
                get => _spacing;
                set
                {
                    _spacing = value;
                    if (_spacing < 1)
                        _spacing = 1;
                }
            }

            /// <summary>
            /// Gets the width
            /// </summary>
            protected double Width
            {
                get
                {
                    if (Variants.Contains(Options.European))
                        return 10;
                    else
                    {
                        if (Height * 0.15 > 5)
                            return 5 + Height * 0.5;
                        return Height * 0.85;
                    }
                }
            }

            /// <summary>
            /// Gets the height
            /// </summary>
            protected double Height
            {
                get
                {
                    double h = _inputs * Spacing;
                    return h < 8 ? 8 : h;
                }
            }

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <inheritdoc />
            Vector2 IBoxDrawable.TopLeft => 0.5 * new Vector2(-Width, -Height);

            /// <inheritdoc />
            Vector2 IBoxDrawable.Center => new();

            /// <inheritdoc />
            Vector2 IBoxDrawable.BottomRight => 0.5 * new Vector2(Width, Height);

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        double r = Width * 0.5;
                        double y = -(_inputs - 1) * Spacing * 0.5;

                        Pins.Clear();
                        char c = 'a';
                        for (int i = 0; i < _inputs; i++)
                        {
                            Pins.Add(new FixedOrientedPin($"input{i}", $"Input {i}", this, new(-r, y), new(-1, 0)), c.ToString(), $"in{i + 1}");
                            y += Spacing;
                            c++;
                        }
                        Pins.Add(new FixedOrientedPin("output", "Output", this, new(r + 3, 0), new(1, 0)), "output", "out", "o");
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawNandIEC(builder, style); break;
                    case 1:
                    default: DrawNand(builder, style); break;
                }
            }
            private void DrawNand(IGraphicsBuilder builder, IStyle style)
            {
                double radius = Height * 0.5;
                double handle = 0.55 * radius;
                double w = Width * 0.5;
                double xr = w - radius;
                double h = Height * 0.5;

                builder.ExtendPins(Pins, style);
                builder.Path(builder => builder
                    .MoveTo(new(-w, h))
                    .LineTo(new(xr, h))
                    .CurveTo(new(xr + handle, h), new(w, handle), new(w, 0))
                    .SmoothTo(new(xr + handle, -h), new(xr, -h))
                    .LineTo(new(-w, -h))
                    .Close(), style);
                builder.Circle(new Vector2(w + 1.5, 0), 1.5, style);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
            private void DrawNandIEC(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);
                builder.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, style);
                builder.Circle(new(Width * 0.5 + 1.5, 0), 1.5, style);

                var span = builder.TextFormatter.Format("&amp;", style);
                builder.Text(span, -span.Bounds.Bounds.Center, TextOrientation.Transformed);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
        }
    }
}
