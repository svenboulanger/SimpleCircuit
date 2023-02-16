using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// And gate.
    /// </summary>
    [Drawable("AND", "An AND gate.", "Digital")]
    public class And : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            private int _inputs = 2;
            private double _spacing = 5;

            /// <inheritdoc />
            public override string Type => "and";

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

            /// <summary>
            /// Spacing between pins
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

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            public override bool Reset(IDiagnosticHandler diagnostics)
            {
                if (!base.Reset(diagnostics))
                    return false;

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
                Pins.Add(new FixedOrientedPin("output", "Output", this, new(r, 0), new(1, 0)), "output", "out", "o");
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawAndIEC(drawing); break;
                    case 1:
                    default: DrawAnd(drawing); break;
                }
            }
            private void DrawAnd(SvgDrawing drawing)
            {
                double radius = Height * 0.5;
                double handle = 0.55 * radius;
                double w = Width * 0.5;
                double xr = w - radius;
                double h = Height * 0.5;

                drawing.ExtendPins(Pins);
                drawing.Path(builder => builder
                    .MoveTo(new(-w, h))
                    .LineTo(new(xr, h))
                    .CurveTo(new(xr + handle, h), new(w, handle), new(w, 0))
                    .SmoothTo(new(xr + handle, -h), new(xr, -h))
                    .LineTo(new(-w, -h))
                    .Close()
                );
                drawing.Text(Labels[0], new(0, -h - 1), new(0, -1));
            }
            private void DrawAndIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Rectangle(Width, Height);
                drawing.Text("&amp;", new(), new());
                drawing.Text(Labels[0], new(0, -Height * 0.5 - 1), new(0, -1));
            }
        }
    }
}