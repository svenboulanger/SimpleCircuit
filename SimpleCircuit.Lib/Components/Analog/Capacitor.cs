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
        private const string _polar = "polar";
        private const string _signs = "signs";
        private const string _programmable = "programmable";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            if (options?.PolarCapacitors ?? false)
            {
                device.AddVariant("polar");
                device.AddVariant("signs");
            }
            return device;
        }

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

                DrawingVariants = Variant.All(
                    Variant.If(_polar).Then(
                        Variant.All(
                            Variant.Do(DrawPolar),
                            Variant.If(_signs).Then(DrawPolarSigns)))
                    .Else(
                        Variant.All(
                            Variant.Do(DrawApolar),
                            Variant.If(_signs).Then(DrawApolarSigns))),
                    Variant.If(_programmable).Then(DrawProgrammable));
            }
            private void DrawPolar(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.ExtendPin(Pins[0], 3.5);
                if (Pins[1].Connections == 0)
                    drawing.ExtendPin(Pins[1], 3.5);

                // Plates
                drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                drawing.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
            }
            private static void DrawPolarSigns(SvgDrawing drawing)
                => CommonGraphical.Signs(drawing, new(-4, 3), new(5, 3), vertical: true);
            private void DrawApolar(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.ExtendPin(Pins[0], 3.5);
                if (Pins[1].Connections == 0)
                    drawing.ExtendPin(Pins[1], 3.5);

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