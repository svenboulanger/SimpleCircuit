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
        [Description("The label next to the transistor.")]
        public string Label { get; set; }

        [Description("Displays a packaged transistor.")]
        public bool Packaged { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pmos"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Pmos(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("drain", "The drain", this, new Vector2(8, 0), new Vector2(1, 0)), "d", "drain");
            Pins.Add(new FixedOrientedPin("gate", "The gate.", this, new Vector2(0, 11), new Vector2(0, 1)), "g", "gate");
            Pins.Add(new FixedOrientedPin("bulk", "The bulk.", this, new Vector2(0, 0), new Vector2(0, -1)), "b", "bulk");
            Pins.Add(new FixedOrientedPin("source", "The source.", this, new Vector2(-8, 0), new Vector2(-1, 0)), "s", "source");
            Packaged = options?.PackagedTransistors ?? false;
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
                // Wires
                drawing.Segments(new Vector2[]
                {
                    new(-8, 0), new(-5, 0),
                    new(8, 0), new(5, 0),
                    new(0, 11), new(0, 6)
                }, new("wire"));

                // Gate
                drawing.Segments(new Vector2[]
                {
                    new(-6, 6), new(6, 6),
                    new(-7, 4), new(-4, 4),
                    new(-2, 4), new(2, 4),
                    new(4, 4), new(7, 4),
                });

                // Drain, source and gate
                drawing.Line(new(-5, 0), new(-5, 4), new("source"));
                drawing.Line(new(5, 0), new(5, 4), new("drain"));
                drawing.Line(new(0, 4), new(0, 0), new("bulk") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
                drawing.Line(new(0, 0), new(-5, 0), new("bulk"));

                // Packaged
                drawing.Circle(new(0, 3), 8.0);

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(3, -10), new(1, 1));
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"PMOS {Name}";
    }
}
