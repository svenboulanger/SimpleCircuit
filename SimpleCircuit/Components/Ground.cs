using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    /// <seealso cref="Component" />
    [SimpleKey("GND")]
    public class Ground : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny;

        /// <inheritdoc/>
        public string Name { get; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => _nx;
        public Function NormalY => _ny;
        public Function MirrorScale => 1.0;

        public PinCollection Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        public Ground(string name)
        {
            Name = name;
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.X);
            _ny = new Unknown(name + ".ny", UnknownTypes.Y);
            Pins = new PinCollection(this);
            Pins.Add(new[] { ".", "a" }, new Vector2(), new Vector2(0, 1));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular);
            drawing.Segments(tf.Apply(new Vector2[]
            {
                new Vector2(0, 0), new Vector2(0, -3),
                new Vector2(-5, -3), new Vector2(5, -3),
                new Vector2(-3, -5), new Vector2(3, -5),
                new Vector2(-1, -7), new Vector2(1, -7)
            }));
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
        public override string ToString() => $"Ground {Name}";
    }
}
