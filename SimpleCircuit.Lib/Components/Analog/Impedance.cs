using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An impedance/admittance.
    /// </summary>
    [Drawable("Z", "An impedance.", "Analog")]
    [Drawable("Y", "An admittance.", "Analog")]
    public class Impedance : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the element.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "neg", "b");

                DrawingVariants = Variant.All(
                    Variant.Do(DrawImpedance),
                    Variant.If("programmable").Do(DrawProgrammable));
            }
            private void DrawImpedance(SvgDrawing drawing)
            {
                // Wires
                drawing.Segments(new[]
                {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            }, new("wire"));

                // The rectangle
                drawing.Polygon(new Vector2[] { new(-6, 3), new(6, 3), new(6, -3), new(-6, -3) });

                // Depending on the orientation, let's anchor the text differently
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private void DrawProgrammable(SvgDrawing drawing)
                => drawing.Line(new(-5, 5), new(6, -7), new("arrow") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
        }
    }
}
