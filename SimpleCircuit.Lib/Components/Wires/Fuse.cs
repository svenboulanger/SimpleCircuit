using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A fuse.
    /// </summary>
    [Drawable("FUSE", "A fuse.", "Wires")]
    public class Fuse : DrawableFactory
    {
        private const string _alt = "alt";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);


        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new(2);

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            /// <inheritdoc />
            public override string Type => "fuse";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawIEC(drawing); break;
                    case 1:
                    default:
                        if (Variants.Contains(_alt))
                            DrawANSIalt(drawing);
                        else
                            DrawANSI(drawing);
                        break;
                }
            }
            private void DrawIEC(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(-6, -3, 12, 6);
                drawing.Path(b => b.MoveTo(-3.5, -3).Line(0, 6).MoveTo(3.5, -3).Line(0, 6));

                Labels.Draw(drawing, 0, new(0, -4), new(0, -1));
                Labels.Draw(drawing, 1, new(0, 4), new(0, 1));
            }
            private void DrawANSI(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(-6, -3, 12, 6);
                drawing.Line(new(-6, 0), new(6, 0));

                Labels.Draw(drawing, 0, new(0, -4), new(0, -1));
                Labels.Draw(drawing, 1, new(0, 4), new(0, 1));
            }
            private void DrawANSIalt(SvgDrawing drawing)
            {
                drawing.OpenBezier(new Vector2[]
                {
                    new(-6, 0),
                    new(-6, -1.65685424949), new(-4.65685424949, -3), new(-3, -3),
                    new(-1.34314575051, -3), new(0, -1.65685424949), new(),
                    new(0, 1.65685424949), new(1.34314575051, 3), new(3, 3),
                    new(4.65685424949, 3), new(6, 1.65685424949), new(6, 0)
                });

                Labels.Draw(drawing, 0, new(0, -4), new(0, -1));
                Labels.Draw(drawing, 1, new(0, 4), new(0, 1));
            }
        }
    }
}