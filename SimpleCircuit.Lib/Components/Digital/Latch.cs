using SimpleCircuit.Components.Builders;
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

        private class Instance : ScaledOrientedDrawable, IBoxDrawable
        {
            /// <inheritdoc />
            public override string Type => "latch";

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxDrawable.TopLeft => new(-9, -12);
            Vector2 IBoxDrawable.BottomRight => new(9, 12);

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, this, 2, "s", "r", "q");

                // Body
                builder.Rectangle(-9, -12, 18, 24, new());

                // Labels
                builder.Text("S", new Vector2(-8, -6), new Vector2(1, 0), Appearance);
                builder.Text("R", new Vector2(-8, 6), new Vector2(1, 0), Appearance);
                builder.Text("Q", new Vector2(8, -6), new Vector2(-1, 0), Appearance);

                if (Pins["nq"].Connections > 0)
                    builder.Text("\\overline{Q}", new Vector2(8, 6), new Vector2(-1, 0), Appearance);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this);
            }
        }
    }
}
