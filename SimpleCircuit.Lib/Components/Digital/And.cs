using SimpleCircuit.Components.Pins;

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
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable
        {
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("a", "The first input.", this, new(-6, -2.5), new(-1, 0)), "a");
                Pins.Add(new FixedOrientedPin("b", "The second input.", this, new(-6, 2.5), new(-1, 0)), "b");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(6, 0), new(1, 0)), "o", "out", "output");
                DrawingVariants = Variant.Do(DrawAnd);
            }
            private void DrawAnd(SvgDrawing drawing)
            {
                drawing.Path(builder => builder
                    .Move(new(-6, 5)).Line(new(7, 0))
                    .Curve(new(3, 0), new(5, -3), new(5, -5))
                    .Smooth(new(-2, -5), new(-5, -5))
                    .Line(new(-7, 0)).Close()
                );
            }
        }
    }
}