using SimpleCircuit.Components.Pins;

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
            var device = new Instance(name, options);
            if (key == "SGND" || (options?.SmallSignal ?? false))
                device.Variants.Add("signal");
            return device;
        }

        private class Instance : ScaledOrientedDrawable
        {
            /// <inheritdoc />
            public override string Type => "ground";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("p", "The one and only pin.", this, new(0, 0), new(0, -1)), "a", "p");
            }
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select("earth", "signal"))
                {
                    case 0: DrawEarth(drawing); break;
                    case 1: DrawSignalGround(drawing); break;
                    default: DrawGround(drawing); break;
                }
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
