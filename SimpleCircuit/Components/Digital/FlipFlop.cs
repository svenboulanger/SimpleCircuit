namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// A flip-flop.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("FF", "Flip-flop", Category = "Digital")]
    public class FlipFlop : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlipFlop"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FlipFlop(string name)
            : base(name)
        {
            Pins.Add(Name, new[] { "d" }, "Data.", new Vector2(-8, 6), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "c", "clk" }, "Clock.", new Vector2(-8, -6), new Vector2(-1, 0));
            Pins.Add(Name, new[] { "r", "rst" }, "Reset.", new Vector2(0, -11), new Vector2(0, -1));
            Pins.Add(Name, new[] { "s", "set" }, "Set.", new Vector2(0, 11), new Vector2(0, 1));
            Pins.Add(Name, new[] { "nq", "qn" }, "Inverted output.", new Vector2(8, -6), new Vector2(1, 0));
            Pins.Add(Name, new[] { "q" }, "Output", new Vector2(8, 6), new Vector2(1, 0));

            // This is just for having a bit nicer initial values
            UnknownScale.Value = -1;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Polygon(new[]
            {
                new Vector2(-8, 11), new Vector2(8, 11),
                new Vector2(8, -11), new Vector2(-8, -11)
            });
            drawing.Polyline(new[]
            {
                new Vector2(-8, -4), new Vector2(-6, -6), new Vector2(-8, -8)
            });

            drawing.Text("D", new Vector2(-7, 5.5), new Vector2(1, 0), 3, 0.5);
            drawing.Text("C", new Vector2(-5, -5.5), new Vector2(1, 0), 3, 0.5);
            drawing.Text("Q", new Vector2(7, 5.5), new Vector2(-1, 0), 3, 0.5);
            if (Pins.IsUsed("nq"))
                drawing.Text("NQ", new Vector2(7, -5.5), new Vector2(-1, 0), 3, 0.5);
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(10, 8), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Flip-flop {Name}";
    }
}
