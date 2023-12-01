using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A latch.
    /// </summary>
    [Drawable("LATCH", "A general latch.", "Digital", "level trigger")]
    public class Latch : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IBoxLabeled
        {
            /// <inheritdoc />
            public override string Type => "latch";

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
                Pins.Add(new FixedOrientedPin("set", "The set pin.", this, new(-9, -6), new(-1, 0)), "s", "set");
                Pins.Add(new FixedOrientedPin("reset", "The reset pin.", this, new(-9, 6), new(-1, 0)), "r", "reset");
                Pins.Add(new FixedOrientedPin("nq", "The inverted output pin.", this, new(9, 6), new(1, 0)), "nq", "qn");
                Pins.Add(new FixedOrientedPin("q", "The output pin.", this, new(9, -6), new(1, 0)), "q");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "s", "r", "q");

                // Body
                drawing.Rectangle(-9, -12, 18, 24, new());

                // Labels
                drawing.Text("S", new Vector2(-8, -6), new Vector2(1, 0));
                drawing.Text("R", new Vector2(-8, 6), new Vector2(1, 0));
                drawing.Text("Q", new Vector2(8, -6), new Vector2(-1, 0));

                if (Pins["nq"].Connections > 0)
                    drawing.Text("\\overline{Q}", new Vector2(8, 6), new Vector2(-1, 0));

                new OffsetAnchorPoints<IBoxLabeled>(BoxLabelAnchorPoints.Default, 1).Draw(drawing, this);
            }
        }
    }
}
