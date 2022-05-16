using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [Drawable("V", "A voltage source.", "Sources")]
    public class VoltageSource : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the source.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "vs";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");

                if (options?.SmallSignal ?? false)
                    AddVariant("ac");

                DrawingVariants = Variant.All(
                    Variant.If("ac").Then(DrawAC).Else(DrawDC),
                    Variant.Do(DrawSource));
            }
            private void DrawSource(SvgDrawing drawing)
            {
                // Wires
                drawing.Path(b => b.MoveTo(-8, 0).LineTo(-6, 0).MoveTo(6, 0).LineTo(8, 0), new("wire"));

                // Circle
                drawing.Circle(new(0, 0), 6);

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
            }
            private void DrawDC(SvgDrawing drawing)
            {
                drawing.Line(new(-3, -1), new(-3, 1), new("minus"));
                drawing.Path(b => b.MoveTo(3, -1).Line(0, 2).MoveTo(2, 0).Line(2, 0), new("plus"));
            }
            private void DrawAC(SvgDrawing drawing)
            {
                double hy = 1 / Math.Sqrt(2);
                double hx = hy * 2; // Slighly exaggerated curves
                drawing.OpenBezier(new Vector2[]
                {
                    new(0, -3),
                    new(hx, -3 + hy), new(hx, -hy), new(),
                    new(-hx, hy), new(-hx, 3 - hy), new(0, 3)
                });
            }
        }
    }
}