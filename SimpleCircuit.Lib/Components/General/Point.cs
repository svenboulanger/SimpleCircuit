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
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name);

        private class Instance : LocatedDrawable, ILabeled
        {
            [Description("The angle along which the label should extend. 0 degrees will put the text on the right.")]
            public double Angle { get; set; }
            [Description("The label distance from the point. The default is 4.")]
            public double Distance { get; set; } = 4.0;
            [Description("The label next to the point.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "point";

            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedPin(name, "The point.", this, new()), "x", "p", "a");
            }

            protected override void Draw(SvgDrawing drawing)
            {
                int connections = Pins[0].Connections;
                if (connections == 0 || connections > 2)
                    drawing.Circle(new Vector2(), 1, new("dot"));

                if (!string.IsNullOrWhiteSpace(Label))
                {
                    var n = Vector2.Normal(-Angle / 180.0 * Math.PI);
                    drawing.Text(Label, n * Distance, n);
                }
            }
        }
    }
}