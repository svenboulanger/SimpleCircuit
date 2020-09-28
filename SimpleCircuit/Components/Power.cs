using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A power supply.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("POW")]
    public class Power : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets the label.
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
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        public Power(string name)
        {
            Name = name;
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.NormalX);
            _ny = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new[] { ".", "a" }, new Vector2(), new Vector2(0, -1));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular);
            drawing.Line(tf.Apply(new Vector2(0, 0)), tf.Apply(new Vector2(0, 3)));
            drawing.Line(tf.Apply(new Vector2(-5, 3)), tf.Apply(new Vector2(5, 3)), "plane");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, 6)), tf.ApplyDirection(new Vector2(0, 1)));
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
        public override string ToString() => $"Power {Name}";
    }
}
