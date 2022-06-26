using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable(new[] { "INV", "NOT" }, "An invertor.", new[] { "Digital" })]
    public class Invertor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            /// <inheritdoc />
            public override string Type => "invertor";

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
                Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power pin.", this, new(0, -3), new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power pin.", this, new(0, 3), new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(9, 0), new(1, 0)), "out", "output");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
                {
                    case 0: DrawInverterIEC(drawing); break;
                    case 1:
                    default: DrawInverter(drawing); break;
                }
            }
            private void DrawInverter(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");
                drawing.Polygon(new[]
                {
                    new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
                });
                drawing.Circle(new Vector2(7.5, 0), 1.5);

                if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(0, -4), new Vector2(1, -1));
            }

            private void DrawInverterIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "in", "out");

                drawing.Rectangle(8, 10, new());
                drawing.Circle(new(5.5, 0), 1.5);
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
                    SetPinOffset(3, new(7, 0));
                }
                else
                {
                    SetPinOffset(0, new(-6, 0));
                    SetPinOffset(1, new(0, -3));
                    SetPinOffset(2, new(0, 3));
                    SetPinOffset(3, new(9, 0));
                }
            }
        }
    }
}