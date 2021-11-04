using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A point.
    /// </summary>
    [SimpleKey("X", "A point that can connect to multiple wires.", Category = "General")]
    public class Point : LocatedDrawable
    {
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
                drawing.Circle(new Vector2(), 1);
        }
    }
}
