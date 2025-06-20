﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

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
            private readonly CustomLabelAnchorPoints _anchors;

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
                _anchors = new(
                    new LabelAnchorPoint(new(0, -6), new(0, -1)),
                    new LabelAnchorPoint(new(0, 6), new(0, 1)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                if (!Variants.Contains(Options.Arei))
                    builder.ExtendPins(Pins, style);
                builder.Circle(new(), 5, style);

                var span = builder.TextFormatter.Format("M", style);
                builder.Text(span, builder.CurrentTransform.Matrix.Inverse * -span.Bounds.Bounds.Center, Vector2.UX, TextOrientationType.Upright);

                if (Variants.Contains(_signs))
                    builder.Signs(new(-6, -4), new(6, -4), style, upright: true);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}