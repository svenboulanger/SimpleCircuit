using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Nor gate.
    /// </summary>
    [Drawable("NOR", "A NOR gate.", "Digital")]
    public class Nor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            private int _inputs = 2;
            private double _spacing = 5;

            /// <inheritdoc />
            public override string Type => "nor";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

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

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                bool keepLeft = Variants.Contains(Options.European);

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
                    }
                    Pins.Add(new FixedOrientedPin($"input{i}", $"Input {i}", this, new(x, y), new(-1, 0)), c.ToString(), $"in{i + 1}");
                    y += Spacing;
                    c++;
                }
                Pins.Add(new FixedOrientedPin("output", "Output", this, new(w + 3, 0), new(1, 0)), "output", "out", "o");
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawNorIEC(drawing); break;
                    case 1:
                    default: DrawNor(drawing); break;
                }
            }
            private void DrawNor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                double w = Width * 0.5;
                double h = Height * 0.5;

                drawing.Path(b =>
                {
                    b.MoveTo(-w, h);
                    b.LineTo(-w * 0.8, h);
                    b.CurveTo(new(w * 0.2, h), new(w * 0.8, h * 0.3), new(w, 0));
                    b.CurveTo(new(w * 0.8, -h * 0.3), new(w * 0.2, -h), new(-w + 1, -h));
                    b.LineTo(-w, -h);
                    b.CurveTo(new(-w * 0.6, -h / 3), new(-w * 0.6, h / 3), new(-w, h));
                });
                drawing.Circle(new(w + 1.5, 0), 1.5);
                drawing.Label(Labels, 0, new(0, -h - 1), new(0, -1));
            }
            private void DrawNorIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Rectangle(-Width * 0.5, -Height * 0.5, Width, Height, new());
                drawing.Text("&#8805;1", new(), new());
                drawing.Circle(new(Width * 0.5 + 1.5, 0), 1.5);
                drawing.Label(Labels, 0, new(0, -Height * 0.5 - 1), new(0, -1));
            }
        }
    }
}
