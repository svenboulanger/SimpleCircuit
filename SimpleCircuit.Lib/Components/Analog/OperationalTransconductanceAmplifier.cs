using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [Drawable(new[] { "OTA", "TA" }, "A transconductance amplifier.", new[] { "Analog" })]
    public class OperationalTransconductanceAmplifier : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the OTA.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "neg");
                Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "pos");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");

                DrawingVariants = Variant.Do(DrawOTA);
            }

            private void DrawOTA(SvgDrawing drawing)
            {
                // The triangle
                drawing.Polygon(new[] {
                new Vector2(-5, -8),
                new Vector2(5, -4),
                new Vector2(5, 4),
                new Vector2(-5, 8)
            });

                // Signs
                drawing.Line(new(-3, -4), new(-1, -4), new("minus"));
                drawing.Segments(new Vector2[] {
                new(-2, 5), new(-2, 3),
                new(-3, 4), new(-1, 4)
            }, new("plus"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
            }
        }
    }
}