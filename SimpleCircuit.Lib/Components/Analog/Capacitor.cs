using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [Drawable("C", "A capacitor.", "Analog")]
    public class Capacitor : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the capacitor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "capacitor";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(5, 0), new(1, 0)), "n", "neg", "b");
                if (options?.PolarCapacitors ?? false)
                    AddVariant("polar");
                DrawingVariants = Variant.All(
                    Variant.If("polar").DoElse(DrawPolar, DrawApolar),
                    Variant.If("programmable").Do(DrawProgrammable));
            }
            private void DrawPolar(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(-1.5, 0).MoveTo(1, 0).LineTo(5, 0), new("wire"));
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                drawing.Path(b => b.MoveTo(new(2.5, -4)).CurveTo(new(1, -2), new(1, -0.5), new(1, 0)).SmoothTo(new(1, 2), new(2.5, 4)), new("neg"));
                drawing.Path(b => b.MoveTo(new(-4, 2)).Line(new(0, 2)).MoveTo(new(-5, 3)).Line(new(2, 0)), new("plus"));
                drawing.Line(new(5, 2), new(5, 4), new("minus"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private void DrawApolar(SvgDrawing drawing)
            {
                // Wires
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(-1.5, 0).MoveTo(1.5, 0).LineTo(5, 0), new("wire"));
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                drawing.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private void DrawProgrammable(SvgDrawing drawing)
            {
                var options = new PathOptions() { EndMarker = PathOptions.MarkerTypes.Arrow };
                drawing.Line(new(-4, 4), new(6, -5), options);
            }
        }
    }
}