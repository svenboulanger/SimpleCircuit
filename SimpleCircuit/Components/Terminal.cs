using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A terminal (input or output).
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("T", "Terminal"), SimpleKey("P", "Pin")]
    public class Terminal : IComponent, ITranslating, IRotating, ILabeled
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <inheritdoc/>
        public PinCollection Pins { get; }

        /// <inheritdoc/>
        public Function X { get; }

        /// <inheritdoc/>
        public Function Y { get; }

        /// <inheritdoc/>
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Terminal(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            X = new Unknown(name + ".x", UnknownTypes.X);
            Y = new Unknown(name + ".y", UnknownTypes.Y);
            NormalX = new Unknown(name + ".nx", UnknownTypes.NormalX);
            NormalY = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Pins = new PinCollection(this);
            Pins.Add(new string[] { "p", "a", "o", "i" }, "The pin.", new Vector2(), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X) + new Squared(Y);
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular);

            drawing.Line(tf.Apply(new Vector2()), tf.Apply(new Vector2(-4, 0)));
            drawing.Circle(tf.Apply(new Vector2(-5.5, 0)), 1.5, "terminal");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(-10, 0)), tf.ApplyDirection(new Vector2(-1, 0)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Terminal {Name}";
    }
}
