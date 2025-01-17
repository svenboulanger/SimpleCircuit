﻿using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A motor.
    /// </summary>
    [Drawable("MOTOR", "A motor.", "Outputs")]
    public class Motor : DrawableFactory
    {
        private const string _signs = "signs";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private static readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -6), new(0, -1)),
                new LabelAnchorPoint(new(0, 6), new(0, 1)));

            /// <inheritdoc />
            public override string Type => "motor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(5, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                if (!Variants.Contains(Options.Arei))
                    builder.ExtendPins(Pins);
                builder.Circle(new(), 5);
                builder.Text("M", new(), new());

                if (Variants.Contains(_signs))
                {
                    builder.Path(b => b
                            .MoveTo(new(-7, -4))
                            .LineTo(new(-5, -4))
                            .MoveTo(new(-6, -3))
                            .LineTo(new(-6, -5)),
                        new("plus"));
                    builder.Line(new(5, -4), new(7, -4), new("minus"));
                }

                _anchors.Draw(builder, this);
            }
        }
    }
}