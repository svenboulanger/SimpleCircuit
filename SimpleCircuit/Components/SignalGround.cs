namespace SimpleCircuit.Components
{
    /// <summary>
    /// An earth terminal.
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    [SimpleKey("SGND", "Signal ground")]
    public class SignalGround : RotatingComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Earth"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SignalGround(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "a" }, "The pin.", new Vector2(), new Vector2(0, 1));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Line(new Vector2(0, 0), new Vector2(0, 3));
            drawing.Polygon(new[]
            {
                new Vector2(-5, 3), new Vector2(5, 3), new Vector2(0, 7)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Earth {Name}";
    }
}
