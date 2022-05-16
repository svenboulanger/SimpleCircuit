using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An inductor.
    /// </summary>
    [Drawable("L", "An inductor.", "Analog")]
    public class Inductor : DrawableFactory
    {
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
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "b");

                DrawingVariants = Variant.All(
                    Variant.Do(DrawInductor),
                    Variant.If("dot").Then(DrawDot),
                    Variant.If("programmable").Then(DrawProgrammable));
            }

            /// <inheritdoc />
            private void DrawInductor(SvgDrawing drawing)
            {
                drawing.Path(b => b
                    .MoveTo(-8, 0).LineTo(-6, 0)
                    .MoveTo(6, 0).LineTo(8, 0), new("wire"));
                drawing.Path(b => b
                    .MoveTo(-6, 0)
                    .CurveTo(new(-6, -4), new(-2, -4), new(-2, 0))
                    .SmoothTo(new(-4, 4), new(-4, 0))
                    .SmoothTo(new(1, -4), new(1, 0))
                    .SmoothTo(new(-1, 4), new(-1, 0))
                    .SmoothTo(new(4, -4), new(4, 0))
                    .SmoothTo(new(2, 4), new(2, 0))
                    .SmoothTo(new(6, -4), new(6, 0)));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
            }
            private void DrawDot(SvgDrawing drawing)
                => drawing.Circle(new(-8, 3.5), 1, new("dot"));
            private void DrawProgrammable(SvgDrawing drawing)
                => drawing.Line(new(-5, 5), new(6, -7), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
        }
    }
}