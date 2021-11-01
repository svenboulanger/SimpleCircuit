using SimpleCircuit.Components.Pins;
using System;
using System.Linq;

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
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the throwing position.
        /// </summary>
        public double Throw { get; set; } = 1.0;

        /// <summary>
        /// If <c>true</c>, the throw pins are swapped.
        /// </summary>
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
        public SwitchSinglePoleDoubleThrow(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("pole", "The pole pin.", this, new(-8, 0), new(-1, 0)), "p", "pole");
            Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 6), new(0, 1)), "c", "ctrl");
            Pins.Add(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(8, 4), new(1, 0)), "t1");
            Pins.Add(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(8, -4), new(1, 0)), "t2");
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
                drawing.Line(new Vector2(-4, 0), new Vector2(4, _swapThrows ? -4 : 4));
            else
                drawing.Line(new Vector2(-4, 0), new Vector2(4, _swapThrows ? 4 : -4));

            if (Pins["c"].Connections > 0)
            {
                if (Throw.IsZero())
                    drawing.Line(new Vector2(0, 0), new Vector2(0, 6));
                else if (Throw > 0)
                    drawing.Line(new(0, _swapThrows ? -2 : 2), new(0, 6));
                else
                    drawing.Line(new(0, _swapThrows ? 2 : -2), new(0, 6));
            }
        }
    }
}
