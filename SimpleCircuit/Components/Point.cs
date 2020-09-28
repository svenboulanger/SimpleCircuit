using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A point that can serve as an inflection point or a node for multiple wires.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("X")]
    public class Point : IComponent
    {
        private readonly Unknown _x, _y;

        /// <inheritdoc/>
        public string Name { get; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => 0.0;
        public Function NormalY => -1.0;
        public Function MirrorScale => 1.0;

        public PinCollection Pins { get; }

        /// <summary>
        /// Gets or sets the number of wires that are connected to this point.
        /// </summary>
        /// <value>
        /// The wires.
        /// </value>
        public int Wires { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Point(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            Pins = new PinCollection(this);
            Pins.Add(new[] { ".", "a", "p" }, new Vector2(), new Vector2(0, -1));
            Wires = 0;
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            // If there are more than 2 wires, then let's draw a point
            if (Wires > 2)
                drawing.Circle(new Vector2(_x.Value, _y.Value), 1);
        }

        /// <summary>
        /// Applies some functions to the minimizer if necessary.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(_x) + new Squared(_y);
        }

        public override string ToString() => $"Point {Name}";
    }
}
