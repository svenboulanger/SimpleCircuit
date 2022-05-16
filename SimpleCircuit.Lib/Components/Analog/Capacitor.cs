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
                Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-1.5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(1.5, 0), new(1, 0)), "n", "neg", "b");
                if (options?.PolarCapacitors ?? false)
                {
                    AddVariant("polar");
                    AddVariant("signs");
                }
                DrawingVariants = Variant.All(
                    Variant.If("polar").Then(
                        Variant.All(
                            Variant.Do(DrawPolar),
                            Variant.If("signs").Then(DrawPolarSigns)))
                    .Else(
                        Variant.All(
                            Variant.Do(DrawApolar),
                            Variant.If("signs").Then(DrawApolarSigns))),
                    Variant.If("programmable").Then(DrawProgrammable));
            }
            private void DrawPolar(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-1.5, 0), new(-5, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(1.5, 0), new(5, 0), new("wire"));

                // Plates
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                drawing.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private static void DrawPolarSigns(SvgDrawing drawing)
                => CommonGraphical.Signs(drawing, new(-4, 3), new(5, 3), vertical: true);
            private void DrawApolar(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-1.5, 0), new(-5, 0), new("wire"));
                if (Pins[1].Connections == 0)
                    drawing.Line(new(1.5, 0), new(5, 0), new("wire"));

                // Plates
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                drawing.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private static void DrawApolarSigns(SvgDrawing drawing)
                => drawing.Signs(new(-4, 3), new(4, 3), vertical: true);
            private void DrawProgrammable(SvgDrawing drawing)
                => drawing.Arrow(new(-4, 4), new(6, -5));
        }
    }
}