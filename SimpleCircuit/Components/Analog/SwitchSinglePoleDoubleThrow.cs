namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// Single-pole double throw switch.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("SPDT", "Single-pole double throw switch", Category = "Analog")]
    public class SwitchSinglePoleDoubleThrow : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the throwing position.
        /// </summary>
        /// <value>
        /// The throw.
        /// </value>
        public double Throw { get; set; } = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchSinglePoleDoubleThrow"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SwitchSinglePoleDoubleThrow(string name)
            : base(name)
        {
            Pins.Add(new[] { "p" }, "The pole pin.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "c", "ctrl" }, "The controlling pin.", new Vector2(0, 6), new Vector2(0, 1));
            Pins.Add(new[] { "t1" }, "The first throwing pin.", new Vector2(8, 4), new Vector2(1, 0));
            Pins.Add(new[] { "t2" }, "The second throwing pin.", new Vector2(8, -4), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 4), new Vector2(8, 4),
                new Vector2(6, -4), new Vector2(8, -4)
            });
            drawing.Circle(new Vector2(-5, 0), 1);
            drawing.Circle(new Vector2(5, 4), 1);
            drawing.Circle(new Vector2(5, -4), 1);

            if (Throw.IsZero())
                drawing.Line(new Vector2(-4, 0), new Vector2(5, 0));
            else if (Throw > 0)
                drawing.Line(new Vector2(-4, 0), new Vector2(4, 4));
            else
                drawing.Line(new Vector2(-4, 0), new Vector2(4, -4));

            if (Pins.IsUsed("c"))
                drawing.Line(new Vector2(0, 2), new Vector2(0, 6));
        }
    }
}
