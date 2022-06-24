using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Nand gate.
    /// </summary>
    [Drawable("NAND", "A NAND gate.", "Digital")]
    public class Nand : DrawableFactory
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
            public override string Type => "nand";

            /// <inheritdoc />
            public string Label { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Or"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="options">Options that can be used for the component.</param>
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-6, -2.5), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-6, 2.5), new(-1, 0)), "b");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(9, 0), new(1, 0)), "o", "output");
                Variants.Changed += UpdatePins;
            }

            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
                {
                    case 0: DrawNandIEC(drawing); break;
                    case 1:
                    default: DrawNand(drawing); break;
                }
            }
            private void DrawNand(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.ClosedBezier(new[]
                {
                    new Vector2(-6, 5),
                    new Vector2(-6, 5), new Vector2(1, 5), new Vector2(1, 5),
                    new Vector2(4, 5), new Vector2(6, 2), new Vector2(6, 0),
                    new Vector2(6, -2), new Vector2(4, -5), new Vector2(1, -5),
                    new Vector2(1, -5), new Vector2(-6, -5), new Vector2(-6, -5)
                });
                drawing.Circle(new Vector2(7.5, 0), 1.5);

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void DrawNandIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(8, 10, new());
                drawing.Circle(new(5.5, 0), 1.5);
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
                    SetPinOffset(2, new(7, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, -2.5));
                    SetPinOffset(1, new(-6, 2.5));
                    SetPinOffset(2, new(9, 0));
                }
            }
        }
    }
}
