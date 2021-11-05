using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    [SimpleKey("S", "A (voltage-controlled) switch. The controlling pin is optional.", Category = "Analog")]
    public class Switch : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the switch.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Switch"/> is closed.
        /// </summary>
        [Description("Closes the switch.")]
        public bool Closed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Switch"/> is opened.
        /// </summary>
        [Description("Opens the switch.")]
        public bool Opened { get => !Closed; set => Closed = !value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Switch(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-8, 0), new(-1, 0)), "p", "a");
            Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 6), new(0, 1)), "c", "ctrl");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(8, 0), new(1, 0)), "n", "b");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            });
            drawing.Circle(new Vector2(-5, 0), 1);
            drawing.Circle(new Vector2(5, 0), 1);

            if (Closed)
                drawing.Line(new Vector2(-4, 0), new Vector2(4, 0));
            else
                drawing.Line(new Vector2(-4, 0), new Vector2(4, 4));

            if (Pins["c"].Connections > 0)
            {
                if (Closed)
                    drawing.Line(new Vector2(0, 0), new Vector2(0, 6));
                else
                    drawing.Line(new Vector2(0, 2), new Vector2(0, 6));
            }

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -6), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Switch {Name}";
    }
}
