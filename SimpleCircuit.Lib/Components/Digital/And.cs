using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// And gate.
    /// </summary>
    [Drawable("AND", "An AND gate.", "Digital")]
    public class And : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            if (options.IEC)
                device.Variants.Add(Options.Iec);
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public override string Type => "and";

            /// <inheritdoc />
            public string Label { get; set; }

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-6, -2.5), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-6, 2.5), new(-1, 0)), "b");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(6, 0), new(1, 0)), "o", "out", "output");
                Variants.Changed += UpdatePins;
            }
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
                {
                    case 0: DrawAndIEC(drawing); break;
                    case 1:
                    default: DrawAnd(drawing); break;
                }
            }
            private void DrawAnd(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Path(builder => builder
                    .MoveTo(new(-6, 5)).Line(new(7, 0))
                    .Curve(new(3, 0), new(5, -3), new(5, -5))
                    .Smooth(new(-2, -5), new(-5, -5))
                    .Line(new(-7, 0)).Close()
                );

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void DrawAndIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(8, 10, new());
                drawing.Text("&amp;", new(), new());

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void UpdatePins(object sender, EventArgs e)
            {
                if (Variants.Contains(Options.Iec))
                {
                    SetPinOffset(0, new(-4, -2.5));
                    SetPinOffset(1, new(-4, 2.5));
                    SetPinOffset(2, new(4, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, -2.5));
                    SetPinOffset(1, new(-6, 2.5));
                    SetPinOffset(2, new(6, 0));
                }
            }
        }
    }
}