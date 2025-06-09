using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Xor gate.
    /// </summary>
    [Drawable("XOR", "An XOR gate.", "Digital")]
    [Drawable("XNOR", "An XNOR gate.", "Digital")]
    public class Xor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name, key.Equals("XNOR"));

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="invertOutput">If <c>true</c>, the output is inverted.</param>
        private class Instance(string name, bool invertOutput) : ScaledOrientedDrawable(name), IBoxDrawable
        {
            private int _inputs = 2;
            private double _spacing = 5;

            /// <inheritdoc />
            public override string Type => "xor";

            /// <summary>
            /// Gets or sets the number of inputs.
            /// </summary>
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

            /// <summary>
            /// Gets or sets the space between inputs.
            /// </summary>
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
                        double w = _inputs * Spacing;
                        return w < 8 ? 8 : w;
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
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        bool keepLeft = Variants.Select(Options.European, Options.American) switch
                        {
                            0 => true,
                            1 or _ => false
                        };

                        Pins.Clear();
                        char c = 'a';
                        double w = Width * 0.5;
                        double h = Height * 0.5;
                        double x = -w;
                        double y = -(_inputs - 1) * Spacing * 0.5;

                        // Solving cubic equations...
                        for (int i = 0; i < _inputs; i++)
                        {
                            if (!keepLeft)
                            {
                                // Calculate the left side of the curve
                                double t = (1 - y / h) * 0.5;
                                double rt = 1 - t;
                                x = -(rt * rt * rt + t * t * t) * w - 3 * (rt * rt * t + rt * t * t) * w * 0.6;
                                x -= w * 0.3;
                            }
                            Pins.Add(new FixedOrientedPin($"input{i}", $"Input {i}", this, new(x, y), new(-1, 0)), c.ToString(), $"in{i + 1}");
                            y += Spacing;
                            c++;
                        }
                        Pins.Add(new FixedOrientedPin("output", "Output", this, invertOutput ? new(w + 3, 0) : new(w, 0), new(1, 0)), "output", "out", "o");
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
                    case 0: DrawXorIEC(builder, style); break;
                    case 1:
                    default: DrawXor(builder, style); break;
                }
            }
            private void DrawXor(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);

                double w = Width * 0.5;
                double h = Height * 0.5;

                builder.Path(b => b.MoveTo(new(-w, h))
                    .LineTo(new(-w * 0.8, h))
                    .CurveTo(new(w * 0.2, h), new(w * 0.8, h * 0.3), new(w, 0))
                    .CurveTo(new(w * 0.8, -h * 0.3), new(w * 0.2, -h), new(-w + 1, -h))
                    .LineTo(new(-w, -h))
                    .CurveTo(new(-w * 0.6, -h / 3), new(-w * 0.6, h / 3), new(-w, h)), style);
                builder.Path(b => b.MoveTo(new(-w * 1.3, h))
                    .CurveTo(new(-w * 0.9, h / 3), new(-w * 0.9, -h / 3), new(-w * 1.3, -h)), style.AsStroke());
                if (invertOutput)
                    builder.Circle(new(w + 1.5, 0), 1.5, style);
                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }

            private void DrawXorIEC(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);
                builder.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, style);
                if (invertOutput)
                    builder.Circle(new(Width * 0.5 + 1.5, 0), 1.5, style);

                var span = builder.TextFormatter.Format("=1", style);
                builder.Text(span, -span.Bounds.Bounds.Center, TextOrientation.Transformed);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
        }
    }
}
