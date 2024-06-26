﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A multiplexer.
    /// </summary>
    [Drawable("MUX", "A multiplexer.", "Digital")]
    public class Multiplexer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled, IBoxLabeled
        {
            /// <inheritdoc />
            public override string Type => "mux";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxLabeled.TopLeft => new(-5, -8);
            Vector2 IBoxLabeled.BottomRight => new(5, 8);

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("1", "The '1' input.", this, new(-5, -4), new(-1, 0)), "a", "1");
                Pins.Add(new FixedOrientedPin("0", "The '0' input.", this, new(-5, 4), new(-1, 0)), "b", "0");
                Pins.Add(new FixedOrientedPin("c", "The controlling input.", this, new(0, -6), new(0, -1)), "c");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "output");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Polygon(new[] {
                    new Vector2(-5, -8),
                    new Vector2(5, -4),
                    new Vector2(5, 4),
                    new Vector2(-5, 8)
                });

                drawing.Text("1", new Vector2(-4, -4), new Vector2(1, 0), 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));
                drawing.Text("0", new Vector2(-4, 4), new Vector2(1, 0), 0.8 * SvgDrawing.DefaultFontSize * Scale, options: new("small"));

                new OffsetAnchorPoints<IBoxLabeled>(BoxLabelAnchorPoints.Default, 1).Draw(drawing, this);
            }
        }
    }
}
