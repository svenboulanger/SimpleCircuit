using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [Drawable("WP", "A wall plug.", "Outputs", "earth child proof sealed")]
    public class Plug : DrawableFactory
    {
        private const string _earth = "earth";
        private const string _sealed = "sealed";
        private const string _child = "child";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "plug";

            [Description("The multiplicity of the wall plug.")]
            [Alias("m")]
            public int Multiple { get; set; } = 1;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(), new(-1, 0)), "in", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(), new(1, 0)), "out", "b");
                _anchors = new(
                    new LabelAnchorPoint(new(6, -1), new(1, -1)));
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);
                builder.ExtendPin(Pins["a"], style);
                builder.Path(b => b
                    .MoveTo(new(4, -4))
                    .ArcTo(4, 4, 0, true, false, new(4, 4)), style);

                if (Variants.Contains(_earth))
                    DrawProtectiveConnection(builder, style);
                if (Variants.Contains(_sealed))
                    DrawSealed(builder, style);
                if (Variants.Contains(_child))
                    DrawChildProtection(builder, style);

                if (Multiple > 1)
                {
                    builder.Line(new(2.6, -1.4), new(-0.2, -4.2), style);
                    builder.Text(Multiple.ToString(), new(-0.6, -4.6), new(-1, -1), style);
                }

                _anchors.Draw(builder, this, style);
            }
            private void DrawProtectiveConnection(IGraphicsBuilder builder, IStyle style)
            {
                builder.Line(new(0, 4), new(0, -4), style);
            }
            private void DrawChildProtection(IGraphicsBuilder builder, IStyle style)
            {
                builder.Path(b => b
                    .MoveTo(new(4, -6))
                    .LineTo(new(4, -4))
                    .MoveTo(new(4, 4))
                    .LineTo(new(4, 6)),
                    style);
            }
            private void DrawSealed(IGraphicsBuilder builder, IStyle style)
            {
                builder.Text("h", new(0.5, 3), new(-1, 1), style);
            }
        }
    }
}