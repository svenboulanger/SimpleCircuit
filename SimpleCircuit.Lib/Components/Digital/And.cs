using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// AND or NAND gate.
    /// </summary>
    [Drawable("AND", "An AND gate.", "Digital")]
    [Drawable("NAND", "A NAND gate.", "Digital")]
    public class And : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name, key.Equals("NAND"));

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name, bool invertOutput) : ScaledOrientedDrawable(name), IBoxDrawable
        {
            private int _inputs = 2;
            private double _spacing = 5;

            /// <inheritdoc />
            public override string Type => invertOutput ? "nand" : "and";

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
            /// Gets or sets the spacing between inputs.
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
            Vector2 IBoxDrawable.TopLeft => -0.5 * new Vector2(Width, Height);

            /// <inheritdoc />
            Vector2 IBoxDrawable.Center => default;

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
                        Pins.Add(new FixedOrientedPin("output", "Output", this, invertOutput ? new(r + 3, 0) : new(r, 0), new(1, 0)), "output", "out", "o");
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
                    case 0: DrawAndIEC(builder, style); break;
                    case 1:
                    default: DrawAnd(builder, style); break;
                }
            }
            private void DrawAnd(IGraphicsBuilder builder, IStyle style)
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

                if (invertOutput)
                    builder.Circle(new(w + 1.5, 0), 1.5, style);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
            private void DrawAndIEC(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);
                builder.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, style);
                if (invertOutput)
                    builder.Circle(new(Width * 0.5 + 1.5, 0), 1.5, style);
                
                var span = builder.TextFormatter.Format("&amp;", style);
                builder.Text(span, -span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.UprightTransformed);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this, style);
            }
        }
    }
}