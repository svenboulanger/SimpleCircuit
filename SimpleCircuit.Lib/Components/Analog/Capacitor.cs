using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [Drawable("C", "A capacitor.", "Analog")]
    public class Capacitor : DrawableFactory
    {
        private const string _curved = "curved";
        private const string _signs = "signs";
        private const string _electrolytic = "electrolytic";
        private const string _programmable = "programmable";
        private const string _sensor = "sensor";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the capacitor.")]
            public string Label { get; set; }

            [Description("The label on the other side of the capacitor.")]
            public string Label2 { get; set; }

            /// <inheritdoc />
            public override string Type => "capacitor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-1.5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(1.5, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            public override void Reset()
            {
                base.Reset();
                if (Variants.Contains(_electrolytic))
                {
                    SetPinOffset(Pins["a"], new(-2.25, 0));
                    SetPinOffset(Pins["b"], new(2.25, 0));
                }
                else
                {
                    SetPinOffset(Pins["a"], new(-1.5, 0));
                    SetPinOffset(Pins["b"], new(1.5, 0));
                }
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3.5);
                double y = 0, y2 = 0;

                switch (Variants.Select(_curved, _electrolytic))
                {
                    case 0:
                        // Plates
                        drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        drawing.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-4, 3), new(5, 3), vertical: true);
                        y = Math.Min(y, -6);
                        y2 = Math.Max(y2, 6);
                        break;

                    case 1:
                        // Assymetric plates
                        drawing.Rectangle(1.5, 8, new(-1.5, 0), new("pos"));
                        drawing.Rectangle(1.5, 8, new(1.5, 0), new("neg", "dot"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-5, 3), new(5, 3), vertical: true);
                        y = Math.Min(y, -6);
                        y2 = Math.Max(y2, 6);
                        break;

                    default:
                        // Plates
                        drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        drawing.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-4, 3), new(4, 3), vertical: true);
                        y = Math.Min(y, -6);
                        y2 = Math.Max(y2, 6);
                        break;
                }

                switch (Variants.Select(_programmable, _sensor))
                {
                    case 0:
                        drawing.Arrow(new(-4, 4), new(6, -5));
                        y = Math.Min(y, -7);
                        y2 = Math.Max(y2, 6);
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[] { new(-6, 6), new(-4, 6), new(4, -6) });
                        y = Math.Min(y, -8);
                        y2 = Math.Max(y2, 8);
                        break;
                }

                // Label
                drawing.Text(Label, new(0, y), new(0, -1), new("lbl"));
                drawing.Text(Label2, new(0, y2), new(0, 1), new("lbl2"));
            }
        }
    }
}