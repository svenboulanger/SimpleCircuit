using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
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
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(6, -1), new(1, -1)));

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
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPin(Pins["a"]);
                builder.Path(b => b
                    .MoveTo(new(0, -4)).ArcTo(4, 4, 0, true, false, new(0, 4)));

                if (Variants.Contains(_earth))
                    DrawProtectiveConnection(builder);
                if (Variants.Contains(_sealed))
                    DrawSealed(builder);
                if (Variants.Contains(_child))
                    DrawChildProtection(builder);

                if (Multiple > 1)
                {
                    builder.Line(new(2.6, -1.4), new(-0.2, -4.2));
                    builder.Text(Multiple.ToString(), new(-0.6, -4.6), new(-1, -1));
                }

                _anchors.Draw(builder, this);
            }
            private void DrawProtectiveConnection(IGraphicsBuilder builder)
            {
                builder.Line(new(0, 4), new(0, -4), new("earth"));
            }
            private void DrawChildProtection(IGraphicsBuilder builder)
            {
                builder.Path(b => b
                    .MoveTo(new(4, -6))
                    .LineTo(new(4, -4))
                    .MoveTo(new(4, 4))
                    .LineTo(new(4, 6)),
                    new("child"));
            }
            private void DrawSealed(IGraphicsBuilder builder)
            {
                builder.Text("h", new(0.5, 3), new(-1, 1));
            }
        }
    }
}