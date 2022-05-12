using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// Single-pole double throw switch.
    /// </summary>
    [Drawable("SPDT", "A single-pole double throw switch. The controlling pin is optional.", "Analog")]
    public class SinglePoleDoubleThrow : DrawableFactory
    {
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
                Pins.Add(new FixedOrientedPin("pole", "The pole pin.", this, new(-8, 0), new(-1, 0)), "p", "pole");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 6), new(0, 1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(8, 4), new(1, 0)), "t1");
                Pins.Add(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(8, -4), new(1, 0)), "t2");

                DrawingVariants = Variant.Map("t1", "t2", "swap", DrawSwitch);
                PinUpdate = Variant.Map("swap", UpdatePins);
            }

            private void DrawSwitch(SvgDrawing drawing, bool t1, bool t2, bool swapped)
            {
                // Wires
                drawing.Path(b => b.MoveTo(-8, 0).LineTo(-6, 0)
                    .MoveTo(6, 4).LineTo(8, 4)
                    .MoveTo(6, -4).LineTo(8, -4), new("wire"));

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

                // Controlling pin (optional)
                if (Pins["c"].Connections > 0)
                {
                    if (!t1 && !t2)
                        drawing.Line(new(0, 0), new(0, 6), new("wire"));
                    else if (t1)
                        drawing.Line(new(0, swapped ? -2 : 2), new(0, 6), new("wire"));
                    else
                        drawing.Line(new(0, swapped ? 2 : -2), new(0, 6), new("wire"));
                }

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(-6, 6), new(-1, 1));
            }
            private void UpdatePins(bool swapped)
            {
                if (swapped)
                {
                    ((FixedOrientedPin)Pins[2]).Offset = new(8, -4);
                    ((FixedOrientedPin)Pins[3]).Offset = new(8, 4);
                }
                else
                {
                    ((FixedOrientedPin)Pins[3]).Offset = new(8, -4);
                    ((FixedOrientedPin)Pins[2]).Offset = new(8, 4);
                }
            }
        }
    }
}