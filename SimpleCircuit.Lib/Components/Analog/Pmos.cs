using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A PMOS transistor.
    /// </summary>
    [SimpleKey("MP", "A PMOS transistor. The bulk connection is optional.", Category = "Analog")]
    [SimpleKey("PMOS", "A PMOS transistor. The bulk connection is optional.", Category = "Analog")]
    public class Pmos : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value that enables the display of the PMOS as packaged.
        /// </summary>
        public bool Packaged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Pmos(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(8, 0), new Vector2(1, 0)), "d", "drain");
            Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 11), new Vector2(0, 1)), "g", "gate");
            Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 0), new Vector2(0, -1)), "b", "bulk");
            Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-8, 0), new Vector2(-1, 0)), "s", "source");
            Packaged = GlobalOptions.PackagedTransistors;
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            if (!Packaged)
            {
                drawing.Segments(new[]
                {
                    new Vector2(0, 11), new Vector2(0, 9),
                    new Vector2(-6, 6), new Vector2(6, 6),
                    new Vector2(-6, 4), new Vector2(6, 4)
                });
                drawing.Circle(new Vector2(0, 7.5), 1.5);

                drawing.Polyline(new[] { new Vector2(-8, 0), new Vector2(-4, 0), new Vector2(-4, 4) });
                drawing.Polyline(new[] { new Vector2(8, 0), new Vector2(4, 0), new Vector2(4, 4) });

                if (Pins["b"].Connections > 0)
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
                    new Vector2(0, 0), new Vector2(-1, 2),
                    new Vector2(0, 0), new Vector2(1, 2)
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
        public override string ToString() => $"PMOS {Name}";
    }
}
