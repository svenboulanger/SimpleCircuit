using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A point that can serve as an inflection point or a node for multiple wires.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("X")]
    public class Point : IComponent
    {
        private readonly IContributor _x, _y;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPin> Pins { get; }

        /// <inheritdoc/>
        public IEnumerable<IContributor> Contributors => new IContributor[] { _x, _y };

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
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            Pins = new IPin[]
            {
                new Pin(this, _x, _y, 1.0.SX(), 1.0.SY(), 0.0.A(), new Vector2(), 0.0, new[] { ".", "a", "p" })
            };
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            // If there are more than 2 wires, then let's draw a point
            if (Wires > 2)
                drawing.Circle(new Vector2(_x.Value, _y.Value), 1);
        }

        public override string ToString() => $"Point {Name}";
    }
}
