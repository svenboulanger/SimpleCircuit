using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// Single-pole double throw switch.
    /// </summary>
    [SimpleKey("SPDT", "A single-pole double throw switch. The controlling pin is optional.", Category = "Analog")]
    public class SwitchSinglePoleDoubleThrow : ScaledOrientedDrawable, ILabeled
    {
        private bool _swapThrows;

        /// <inheritdoc/>
        [Description("The label next to the switch.")]
        public string Label { get; set; }

        [Description("Sets the position of the switch. -1, 0 or 1.")]
        public double Throw { get; set; } = 1.0;

        [Description("Swaps the two throws.")]
        public bool SwapThrows
        {
            get => _swapThrows;
            set
            {
                _swapThrows = value;
                if (value)
                {
                    ((FixedOrientedPin)Pins[2]).Offset = new(8, -4);
                    ((FixedOrientedPin)Pins[3]).Offset = new(8, 4);
                }
                else
                {
                    ((FixedOrientedPin)Pins[3]).Offset = new(8, -4);
                    ((FixedOrientedPin)Pins[2]).Offset = new(8, 4);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchSinglePoleDoubleThrow"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public SwitchSinglePoleDoubleThrow(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("pole", "The pole pin.", this, new(-8, 0), new(-1, 0)), "p", "pole");
            Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 6), new(0, 1)), "c", "ctrl");
            Pins.Add(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(8, 4), new(1, 0)), "t1");
            Pins.Add(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(8, -4), new(1, 0)), "t2");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 4), new Vector2(8, 4),
                new Vector2(6, -4), new Vector2(8, -4)
            }, new("wire"));

            // Terminals
            drawing.Circle(new(-5, 0), 1);
            drawing.Circle(new(5, 4), 1);
            drawing.Circle(new(5, -4), 1);

            // Switch position
            if (Throw.IsZero())
                drawing.Line(new(-4, 0), new(5, 0));
            else if (Throw > 0)
                drawing.Line(new(-4, 0), new(4, _swapThrows ? -4 : 4));
            else
                drawing.Line(new(-4, 0), new(4, _swapThrows ? 4 : -4));

            // Controlling pin (optional)
            if (Pins["c"].Connections > 0)
            {
                if (Throw.IsZero())
                    drawing.Line(new(0, 0), new(0, 6), new("wire"));
                else if (Throw > 0)
                    drawing.Line(new(0, _swapThrows ? -2 : 2), new(0, 6), new("wire"));
                else
                    drawing.Line(new(0, _swapThrows ? 2 : -2), new(0, 6), new("wire"));
            }

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(-6, 6), new(-1, 1));
        }
    }
}
