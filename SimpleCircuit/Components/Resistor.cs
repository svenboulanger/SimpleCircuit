using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A resistor.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("R")]
    public class Resistor : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny;
        private double _s = 1;

        /// <inheritdoc/>
        public string Name { get; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => _nx;
        public Function NormalY => _ny;
        public Function MirrorScale => _s;

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        /// <inheritdoc/>
        public PinCollection Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Resistor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.NormalX);
            _ny = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new[] { "p", "+", "pos", "a" }, new Vector2(8, 0), new Vector2(1, 0));
            Pins.Add(new[] { "n", "-", "neg", "b" }, new Vector2(-8, 0), new Vector2(-1, 0));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular * _s);
            drawing.Poly(tf.Apply(
                new Vector2[]
                {
                    new Vector2(-8, 0),
                    new Vector2(-6, 0),
                    new Vector2(-5, -4),
                    new Vector2(-3, 4),
                    new Vector2(-1, -4),
                    new Vector2(1, 4),
                    new Vector2(3, -4),
                    new Vector2(5, 4),
                    new Vector2(6, 0),
                    new Vector2(8, 0)
                }));

            // Depending on the orientation, let's anchor the text differently
            drawing.Text(Label, tf.Apply(new Vector2(0, -7)), tf.ApplyDirection(new Vector2(0, -1)));
        }


        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Resistor {Name}";

        /// <summary>
        /// Applies some functions to the minimizer if necessary.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(_x) + new Squared(_y) + new Squared(_nx) + new Squared(_ny - 1);
        }
    }
}
