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
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            switch (options?.Style ?? Options.Styles.ANSI)
            {
                case Options.Styles.AREI:
                    device.Variants.Add(Options.Arei);
                    break;
                case Options.Styles.IEC:
                    device.Variants.Add(Options.Iec);
                    break;
                default:
                    device.Variants.Add(Options.Ansi);
                    break;
            }
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the fuse.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "fuse";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
            }
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Iec, Options.Ansi))
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

                drawing.Rectangle(12, 6, new());
                drawing.Path(b => b.MoveTo(-3.5, -3).Line(0, 6).MoveTo(3.5, -3).Line(0, 6));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -4), new(0, -1));
            }
            private void DrawANSI(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                drawing.Rectangle(12, 6, new());
                drawing.Line(new(-6, 0), new(6, 0));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -4), new(0, -1));
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
            }

        }
    }
}