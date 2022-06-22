using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    [Drawable("GND", "A common ground symbol.", "General")]
    [Drawable("SGND", "A signal ground symbol.", "General")]
    public class Ground : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            switch (key)
            {
                case "GND":
                    return new Instance(name, options);
                case "SGND":
                    var result = new Instance(name, options);
                    result.AddVariant("signal");
                    return result;
                default:
                    throw new ArgumentException($"Could not recognize key '{key}' for ground.");
            }
        }

        private class Instance : ScaledOrientedDrawable
        {
            /// <inheritdoc />
            public override string Type => "ground";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");

                if (options?.SmallSignal ?? false)
                    AddVariant("signal");

                DrawingVariants = Variant.FirstOf(
                    Variant.If("earth").Then(DrawEarth),
                    Variant.If("signal").Then(DrawSignalGround),
                    Variant.Do(DrawGround));
            }
            private void DrawGround(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3);

                // Ground
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(5, 0).MoveTo(-3, 2).LineTo(3, 2).MoveTo(-1, 4).LineTo(1, 4));
            }
            private void DrawEarth(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3);

                // Ground segments
                drawing.Path(b => b.MoveTo(-5, 0).LineTo(5, 0)
                    .MoveTo(-5, 0).Line(-2, 4)
                    .MoveTo(0, 0).Line(-2, 4)
                    .MoveTo(5, 0).Line(-2, 4));
            }
            private void DrawSignalGround(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3);

                // Ground
                drawing.Polygon(new[]
                {
                    new Vector2(-5, 0), new Vector2(5, 0), new Vector2(0, 4)
                });
            }
        }
    }
}
