using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A supply voltage.
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("POW")]
    public class Power : IComponent, ITranslating, IRotating, ILabeled
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Label { get; set; } = "VDD";

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
        public Power(string name)
        {
            Name = name;
            X = new Unknown(name + ".x", UnknownTypes.X);
            Y = new Unknown(name + ".y", UnknownTypes.Y);
            NormalX = new Unknown(name + ".nx", UnknownTypes.NormalX);
            NormalY = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new[] { "a" }, "The pin.", new Vector2(), new Vector2(0, -1));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular);
            drawing.Line(tf.Apply(new Vector2(0, 0)), tf.Apply(new Vector2(0, 3)));
            drawing.Line(tf.Apply(new Vector2(-5, 3)), tf.Apply(new Vector2(5, 3)), "plane");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, 6)), tf.ApplyDirection(new Vector2(0, 1)));
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
        public override string ToString() => $"Power {Name}";
    }
}
