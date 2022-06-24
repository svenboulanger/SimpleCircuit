using SimpleCircuit.Components.Pins;

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
                device.Variants.Add("polar");
                device.Variants.Add("signs");
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
            }
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3.5);

                if (Variants.Contains(_polar))
                {
                    // Plates
                    drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                    drawing.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));
                    if (Variants.Contains(_signs))
                        drawing.Signs(new(-4, 3), new(5, 3), vertical: true);

                    // Label
                    if (!string.IsNullOrWhiteSpace(Label))
                        drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
                }
                else
                {
                    // Plates
                    drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                    drawing.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));
                    if (Variants.Contains(_signs))
                        drawing.Signs(new(-4, 3), new(4, 3), vertical: true);

                    // Label
                    if (!string.IsNullOrWhiteSpace(Label))
                        drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
                }

                if (Variants.Contains(_programmable))
                    drawing.Arrow(new(-4, 4), new(6, -5));
            }
        }
    }
}