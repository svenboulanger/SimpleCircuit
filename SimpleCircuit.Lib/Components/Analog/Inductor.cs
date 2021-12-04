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
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "b");

                DrawingVariants = Variant.All(
                    Variant.Do(DrawInductor),
                    Variant.If("dot").Do(DrawDot),
                    Variant.If("programmable").Do(DrawProgrammable));
            }

            /// <inheritdoc />
            private void DrawInductor(SvgDrawing drawing)
            {
                // Wires
                drawing.Segments(new[]
                {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            }, new("wire"));

                // The coils
                drawing.SmoothBezier(new[]
                {
                new Vector2(-6, 0),
                new Vector2(-6, -4), new Vector2(-2, -4), new Vector2(-2, 0),
                new Vector2(-4, 4), new Vector2(-4, 0),
                new Vector2(1, -4), new Vector2(1, 0),
                new Vector2(-1, 4), new Vector2(-1, 0),
                new Vector2(4, -4), new Vector2(4, 0),
                new Vector2(2, 4), new Vector2(2, 0),
                new Vector2(6, -4), new Vector2(6, 0)
            });

                // The label
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