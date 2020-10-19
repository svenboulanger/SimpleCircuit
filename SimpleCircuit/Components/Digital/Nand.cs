namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// And gate.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    [SimpleKey("NAND", "Nand gate.", Category = "Digital")]
    public class Nand : TransformingComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Nand(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "a", "in1" }, "First input.", new Vector2(-6, 2), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "b", "in2" }, "Second input.", new Vector2(-6, -2), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "o", "out" }, "Output.", new Vector2(9, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.ClosedBezier(new[]
            {
                new Vector2(-6, 5),
                new Vector2(-6, 5), new Vector2(1, 5), new Vector2(1, 5),
                new Vector2(4, 5), new Vector2(6, 2), new Vector2(6, 0),
                new Vector2(6, -2), new Vector2(4, -5), new Vector2(1, -5),
                new Vector2(1, -5), new Vector2(-6, -5), new Vector2(-6, -5)
            });
            drawing.Circle(new Vector2(7.5, 0), 1.5);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"And {Name}";
    }
}
