using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An inductor.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("L")]
    public class Inductor : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny;
        private double _s = 1.0;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => _nx;
        public Function NormalY => _ny;
        public Function MirrorScale => 1.0;

        public PinCollection Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Inductor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.NormalX);
            _ny = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new[] { "p", "+", "pos", "a" }, new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "-", "neg", "b" }, new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular * _s);

            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            }));
            drawing.SmoothBezier(tf.Apply(new[]
            {
                new Vector2(-6, 0),
                new Vector2(-6, -4), new Vector2(-2, -4), new Vector2(-2, 0),
                new Vector2(-4, 4), new Vector2(-4, 0),
                new Vector2(1, -4), new Vector2(1, 0),
                new Vector2(-1, 4), new Vector2(-1, 0),
                new Vector2(4, -4), new Vector2(4, 0),
                new Vector2(2, 4), new Vector2(2, 0),
                new Vector2(6, -4), new Vector2(6, 0)
            }));

            drawing.Text(Label, tf.Apply(new Vector2(0, -6)), tf.ApplyDirection(new Vector2(0, -1)));
        }

        /// <summary>
        /// Applies some functions to the minimizer if necessary.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(_x) + new Squared(_y) + new Squared(_nx) + new Squared(_ny - 1);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Inductor {Name}";
    }
}
