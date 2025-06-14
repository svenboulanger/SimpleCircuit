﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A direction that is like a regular point, but can be oriented.
    /// This is useful for example when combined with subcircuits to give an orientation.
    /// </summary>
    [Drawable("DIR", "Directional point, useful for defining subcircuit definition ports.", "General")]
    public class Direction : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : OrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);

            [Description("The distance of the text to the point.")]
            [Alias("l")]
            [Alias("d")]
            public double Length { get; set; } = 2.0;

            /// <inheritdoc />
            public override string Type => "direction";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(), new(1, 0)), "o", "b", "out", "output");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                _anchors[0] = new LabelAnchorPoint(new(0, -Length), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, Length), new(0, 1));
                _anchors.Draw(builder, this, builder.Style.Modify(Style));
            }
        }
    }
}
