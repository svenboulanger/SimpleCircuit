using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable("BUF", "An invertor.", "Digital")]
    public class Buffer : DrawableFactory
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
            [Description("The label next to the inverter.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "invertor";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power pin.", this, new(0, -3), new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power pin.", this, new(0, 3), new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(6, 0), new(1, 0)), "out", "output");
                Variants.Changed += UpdatePins;
            }
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
                {
                    case 0: DrawBufferIEC(drawing); break;
                    case 1:
                    default: DrawBuffer(drawing); break;
                }
            }
            private void DrawBuffer(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");
                drawing.Polygon(new[]
                {
                    new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
                });

                if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(0, -4), new Vector2(1, -1));
            }

            private void DrawBufferIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");

                drawing.Rectangle(8, 10, new());
                drawing.Text("1", new(), new());

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void UpdatePins(object sender, EventArgs e)
            {
                if (Variants.Contains(Options.Iec))
                {
                    SetPinOffset(0, new(-4, 0));
                    SetPinOffset(1, new(0, -5));
                    SetPinOffset(2, new(0, 5));
                    SetPinOffset(3, new(4, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, 0));
                    SetPinOffset(1, new(0, -3));
                    SetPinOffset(2, new(0, 3));
                    SetPinOffset(3, new(6, 0));
                }
            }
        }
    }
}