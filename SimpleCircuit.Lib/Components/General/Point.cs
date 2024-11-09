using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for points.
    /// </summary>
    [Drawable("X", "A point that can connect to multiple wires.", "General")]
    public class PointFactory : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(new LabelAnchorPoint());

            [Description("The angle along which the label should extend. 0 degrees will put the text on the right.")]
            [Alias("a")]
            public double Angle { get; set; }

            [Description("The label distance from the point. The default is 3.")]
            [Alias("d")]
            [Alias("l")]
            public double Distance { get; set; } = 3.0;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "point";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedPin(name, "The point.", this, new()), "x", "p", "a");
                Variants.Add("dot");
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                if (Variants.Contains("dot"))
                {
                    int connections = Pins[0].Connections;
                    if (Variants.Contains("forced") || connections == 0 || connections > 2)
                    {
                        var marker = new Dot(new(), new(1, 0), new("marker", "dot", "wire"));
                        marker.Draw(builder);
                    }
                }

                var n = Vector2.Normal(-Angle / 180.0 * Math.PI);
                _anchors[0] = new LabelAnchorPoint(n * Distance, n);
                _anchors.Draw(builder, this);
            }
        }
    }
}