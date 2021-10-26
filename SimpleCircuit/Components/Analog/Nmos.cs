namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An NMOS transistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("MN", "NMOS transistor", Category = "Analog"), SimpleKey("NMOS", "NMOS transistor", Category = "Analog")]
    public class Nmos : TransformingComponent, ILabeled
    {
        /// <inheritdoc />
        public string Label { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Packaged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Nmos(string name)
            : base(name)
        {
            Pins.Add(new[] { "s", "source" }, "The source.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "g", "gate" }, "The gate.", new Vector2(0, 8), new Vector2(0, 1));
            Pins.Add(new[] { "b", "bulk" }, "The bulk.", new Vector2(0, 0), new Vector2(0, -1));
            Pins.Add(new[] { "d", "drain" }, "The drain.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            if (Packaged.IsZero())
            {
                drawing.Segments(new[]
                {
                new Vector2(0, 8), new Vector2(0, 6),
                new Vector2(-6, 6), new Vector2(6, 6),
                new Vector2(-6, 4), new Vector2(6, 4)
            });
                drawing.Polyline(new[] { new Vector2(-8, 0), new Vector2(-4, 0), new Vector2(-4, 4) });
                drawing.Polyline(new[] { new Vector2(8, 0), new Vector2(4, 0), new Vector2(4, 4) });

                if (Pins.IsUsed("b"))
                {
                    drawing.Line(new Vector2(0, 4), new Vector2(0, 0));
                    if (!string.IsNullOrEmpty(Label))
                        drawing.Text(Label, new Vector2(-3, -3), new Vector2(-1, -1));
                }
                else if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
            }
            else
            {
                drawing.Segments(new[]
                {
                    new Vector2(0, 11), new Vector2(0, 6),
                    new Vector2(-6, 6), new Vector2(6, 6),
                    new Vector2(-7, 4), new Vector2(-4, 4),
                    new Vector2(-2, 4), new Vector2(2, 4),
                    new Vector2(4, 4), new Vector2(7, 4),
                    new Vector2(0, 4), new Vector2(-1, 2),
                    new Vector2(0, 4), new Vector2(1, 2)
                });
                drawing.Circle(new Vector2(0, 3), 8.0);

                drawing.Polyline(new[] { new Vector2(-8, 0), new Vector2(-5, 0), new Vector2(-5, 4) });
                drawing.Polyline(new[] { new Vector2(8, 0), new Vector2(5, 0), new Vector2(5, 4) });
                drawing.Polyline(new[] { new Vector2(-5, 0), new Vector2(0, 0), new Vector2(0, 4) });
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"NMOS {Name}";
    }
}
