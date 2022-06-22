using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// Single-pole double throw switch.
    /// </summary>
    [Drawable("SPDT", "A single-pole double throw switch. The controlling pin is optional.", "Analog")]
    public class SinglePoleDoubleThrow : DrawableFactory
    {
        private const string _t1 = "t1";
        private const string _t2 = "t2";
        private const string _swap = "swap";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the switch.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "spdt";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("pole", "The pole pin.", this, new(-6, 0), new(-1, 0)), "p", "pole");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 0), new(0, 1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("control2", "The backside controlling pin.", this, new(0, 0), new(0, -1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(6, 4), new(1, 0)), "t1");
                Pins.Add(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(6, -4), new(1, 0)), "t2");

                DrawingVariants = Variant.Map(_t1, _t2, _swap, DrawSwitch);
                PinUpdate = Variant.All(
                    Variant.Map(_swap, UpdatePins),
                    Variant.Map(_swap, _t1, _t2, UpdateControlPin));
            }

            private void DrawSwitch(SvgDrawing drawing, bool t1, bool t2, bool swapped)
            {
                drawing.ExtendPins(Pins, 2, "p", "t1", "t2");

                // Terminals
                drawing.Circle(new(-5, 0), 1);
                drawing.Circle(new(5, 4), 1);
                drawing.Circle(new(5, -4), 1);

                // Switch position
                if (!t1 && !t2)
                    drawing.Line(new(-4, 0), new(5, 0));
                else if (t1)
                    drawing.Line(new(-4, 0), new(4, swapped ? -4 : 4));
                else
                    drawing.Line(new(-4, 0), new(4, swapped ? 4 : -4));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(-6, 6), new(-1, 1));
            }
            private void UpdateControlPin(bool swapped, bool t1, bool t2)
            {
                Vector2 loc;
                if (!t1 && !t2)
                    loc = new();
                else if (t1)
                    loc = new(0, swapped ? -2 : 2);
                else
                    loc = new(0, swapped ? 2 : -2);
                SetPinOffset(1, loc);
                SetPinOffset(2, loc);
            }
            private void UpdatePins(bool swapped)
            {
                if (swapped)
                {
                    SetPinOffset(3, new(6, -4));
                    SetPinOffset(4, new(6, 4));
                }
                else
                {
                    SetPinOffset(3, new(6, -4));
                    SetPinOffset(4, new(6, 4));
                }
            }
        }
    }
}