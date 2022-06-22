using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An inductor.
    /// </summary>
    [Drawable("L", "An inductor.", "Analog")]
    public class Inductor : DrawableFactory
    {
        private const string _dot = "dot";
        private const string _programmable = "programmable";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the inductor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "inductor";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");

                DrawingVariants = Variant.All(
                    Variant.Do(DrawInductor),
                    Variant.If(_dot).Then(DrawDot),
                    Variant.If(_programmable).Then(DrawProgrammable));
            }

            /// <inheritdoc />
            private void DrawInductor(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-6, 0), new(-8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(6, 0), new(8, 0), new("wire"));

                // Inductor
                drawing.Path(b => b
                    .MoveTo(-6, 0)
                    .CurveTo(new(-6, -4), new(-2, -4), new(-2, 0))
                    .SmoothTo(new(-4, 4), new(-4, 0))
                    .SmoothTo(new(1, -4), new(1, 0))
                    .SmoothTo(new(-1, 4), new(-1, 0))
                    .SmoothTo(new(4, -4), new(4, 0))
                    .SmoothTo(new(2, 4), new(2, 0))
                    .SmoothTo(new(6, -4), new(6, 0)));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -5), new Vector2(0, -1));
            }
            private void DrawDot(SvgDrawing drawing) => drawing.Dot(new(-8, 3.5));
            private void DrawProgrammable(SvgDrawing drawing)
                => drawing.Arrow(new(-5, 5), new(6, -7));
        }
    }
}