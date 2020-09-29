using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    /// <seealso cref="Component" />
    [SimpleKey("GND")]
    public class Ground : IComponent, ITranslating, IRotating
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public Function X { get; }

        /// <inheritdoc/>
        public Function Y { get; }

        /// <inheritdoc/>
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <inheritdoc/>
        public PinCollection Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Ground(string name)
        {
            Name = name;
            X = new Unknown(name + ".x", UnknownTypes.X);
            Y = new Unknown(name + ".y", UnknownTypes.Y);
            NormalX = new Unknown(name + ".nx", UnknownTypes.NormalX);
            NormalY = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new[] { ".", "a" }, new Vector2(), new Vector2(0, 1));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular);
            drawing.Segments(tf.Apply(new Vector2[]
            {
                new Vector2(0, 0), new Vector2(0, -3),
                new Vector2(-5, -3), new Vector2(5, -3),
                new Vector2(-3, -5), new Vector2(3, -5),
                new Vector2(-1, -7), new Vector2(1, -7)
            }));
        }

        /// <inheritdoc/>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X) + new Squared(Y);
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
