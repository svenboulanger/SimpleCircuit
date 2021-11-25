using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A point.
    /// </summary>
    [SimpleKey("X", "A point that can connect to multiple wires.", Category = "General")]
    public class Point : LocatedDrawable, ILabeled
    {
        [Description("The angle along which the label should extend. 0 degrees will put the text on the right.")]
        public double Angle { get; set; }

        [Description("The label distance from the point. The default is 4.")]
        public double Distance { get; set; } = 4.0;

        [Description("The label next to the point.")]
        public string Label { get; set; }

        protected override IEnumerable<string> GroupClasses
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Label))
                    yield return "labeled";
            }
        }

        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="name">The name.</param>
        public Point(string name)
            : base(name)
        {
            Pins.Add(new FixedPin(name, "The point.", this, new()), "x", "p", "a");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            int connections = Pins[0].Connections;
            if (connections == 0 || connections > 2)
                drawing.Circle(new Vector2(), 1, new("dot"));

            if (!string.IsNullOrWhiteSpace(Label))
            {
                var n = Vector2.Normal(Angle / 180.0 * Math.PI);
                drawing.Text(Label, n * Distance, n);
            }
        }
    }
}
