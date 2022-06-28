using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// Xor gate.
    /// </summary>
    [Drawable("XNOR", "An XNOR gate.", "Digital")]
    public class Xnor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            /// <inheritdoc />
            public override string Type => "xnor";

            /// <inheritdoc />
            public string Label { get; set; }

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.ANSI | Standards.IEC;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-5.5, -2.5), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-5.5, 2.5), new(-1, 0)), "b");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(9, 0), new(1, 0)), "o", "out", "output");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
                {
                    case 0: DrawXnorIEC(drawing); break;
                    case 1:
                    default: DrawXnor(drawing); break;
                }
            }
            private void DrawXnor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.ClosedBezier(new[]
                {
                    new Vector2(-5, 5),
                    new Vector2(-5, 5), new Vector2(-4, 5), new Vector2(-4, 5),
                    new Vector2(1, 5), new Vector2(4, 3), new Vector2(6, 0),
                    new Vector2(4, -3), new Vector2(1, -5), new Vector2(-4, -5),
                    new Vector2(-4, -5), new Vector2(-5, -5), new Vector2(-5, -5),
                    new Vector2(-3, -2), new Vector2(-3, 2), new Vector2(-5, 5)
                });
                drawing.OpenBezier(new[]
                {
                    new Vector2(-6.5, -5), new Vector2(-4.5, -2), new Vector2(-4.5, 2), new Vector2(-6.5, 5)
                });
                drawing.Circle(new(7.5, 0), 1.5);

                drawing.Text(Label, new(0, -6), new(0, -1));
            }

            private void DrawXnorIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(8, 10, new());
                drawing.Circle(new(5.5, 0), 1.5);
                drawing.Text("=1", new(), new());

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
                    SetPinOffset(0, new(-5.5, -2.5));
                    SetPinOffset(1, new(-5.5, 2.5));
                    SetPinOffset(2, new(9, 0));
                }
            }
        }
    }
}
