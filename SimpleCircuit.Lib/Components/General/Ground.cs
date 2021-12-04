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
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");

                if (options?.SmallSignal ?? false)
                    AddVariant("signal");

                DrawingVariants = Variant.FirstOf(
                    Variant.If("earth").Do(DrawEarth),
                    Variant.If("signal").Do(DrawSignalGround),
                    Variant.Do(DrawGround));
            }
            private void DrawGround(SvgDrawing drawing)
            {
                // Wire
                drawing.Line(new(0, 0), new(0, 3), new("wire"));

                // Ground
                drawing.Segments(new Vector2[]
                {
                new(-5, 3), new(5, 3),
                new(-3, 5), new(3, 5),
                new(-1, 7), new(1, 7)
                });
            }
            private void DrawEarth(SvgDrawing drawing)
            {
                // Wire
                drawing.Line(new(0, 0), new(0, 3), new("wire"));

                // Ground segments
                drawing.Segments(new Vector2[]
                {
                new(-5, 3), new(5, 3),
                new(-5, 3), new(-7, 7),
                new(0, 3), new(-2, 7),
                new(5, 3), new(3, 7)
                });
            }
            private void DrawSignalGround(SvgDrawing drawing)
            {
                // Wires
                drawing.Line(new Vector2(0, 0), new Vector2(0, 3), new("wire"));

                // Ground
                drawing.Polygon(new[]
                {
                new Vector2(-5, 3), new Vector2(5, 3), new Vector2(0, 7)
            });
            }
        }
    }
}
