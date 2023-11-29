using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    [Drawable("FF", "A general flip-flop.", "Digital", "edge trigger")]
    public class FlipFlop : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IBoxLabeled
        {
            /// <inheritdoc />
            public override string Type => "flipflop";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxLabeled.TopLeft => new(-9, -12);
            Vector2 IBoxLabeled.BottomRight => new(9, 12);

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("data", "The data pin.", this, new(-9, -6), new(-1, 0)), "d", "data");
                Pins.Add(new FixedOrientedPin("clock", "The clock pin.", this, new(-9, 6), new(-1, 0)), "c", "clock");
                Pins.Add(new FixedOrientedPin("reset", "The reset pin.", this, new(0, 12), new(0, 1)), "r", "rst", "reset");
                Pins.Add(new FixedOrientedPin("set", "The set pin.", this, new(0, -12), new(0, -1)), "s", "set");
                Pins.Add(new FixedOrientedPin("nq", "The inverted output pin.", this, new(9, 6), new(1, 0)), "nq", "qn");
                Pins.Add(new FixedOrientedPin("q", "The output pin.", this, new(9, -6), new(1, 0)), "q");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "d", "c", "q");

                // Body
                drawing.Rectangle(-9, -12, 18, 24, new());

                // Clock thingy
                drawing.Polyline(new[]
                {
                    new Vector2(-9, 4), new Vector2(-7, 6), new Vector2(-9, 8)
                });
                drawing.Text("D", new Vector2(-8, -6), new Vector2(1, 0));
                drawing.Text("C", new Vector2(-6, 6), new Vector2(1, 0));
                drawing.Text("Q", new Vector2(8, -6), new Vector2(-1, 0));

                if (Pins["nq"].Connections > 0)
                    drawing.Text("Q'", new Vector2(8, 6), new Vector2(-1, 0));
                if (Pins["s"].Connections > 0)
                    drawing.Text("set", new Vector2(0, -11.5), new Vector2(0, 1), 0.8 * SvgDrawing.DefaultFontSize, new("small"));
                if (Pins["r"].Connections > 0)
                    drawing.Text("rst", new Vector2(0, 11.5), new Vector2(0, -1), 0.8 * SvgDrawing.DefaultFontSize, new("small"));

                new OffsetAnchorPoints<IBoxLabeled>(BoxLabelAnchorPoints.Default, 1).Draw(drawing, this);
            }
        }
    }
}