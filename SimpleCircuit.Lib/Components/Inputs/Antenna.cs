using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Inputs
{
    [Drawable("ANT", "An antenna.", "Inputs")]
    public class Antenna : DrawableFactory
    {
        private const string _alt = "alt";

        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the antenna.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "antenna";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("pin", "The pin of the antenna.", this, new(), new(0, 1)), "p", "pin", "a");
                DrawingVariants = Variant.FirstOf(
                    Variant.If(_alt).Then(DrawAntennaAlt),
                    Variant.Do(DrawAntenna));
            }

            private void DrawAntenna(SvgDrawing drawing)
            {
                drawing.Path(b =>
                {
                    b.MoveTo(0, 0);
                    b.Line(0, -10);
                    b.MoveTo(0, -3);
                    b.LineTo(-5, -10);
                    b.MoveTo(0, -3);
                    b.LineTo(5, -10);
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(5, -5), new(1, 0));
            }
            private void DrawAntennaAlt(SvgDrawing drawing)
            {
                drawing.Path(b =>
                {
                    b.MoveTo(0, 0);
                    b.Line(0, -10);
                    b.MoveTo(0, -3);
                    b.LineTo(-5, -10);
                    b.LineTo(5, -10);
                    b.LineTo(0, -3);
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(5, -5), new(1, 0));
            }
        }
    }
}
