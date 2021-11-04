using SimpleCircuit.Components.Pins;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An operational amplifier.
    /// </summary>
    [SimpleKey("OA", "An operational amplifier.", Category = "Analog")]
    public class Opamp : ScaledOrientedDrawable, ILabeled
    {
        private bool _swapInputs = false;
        private static readonly Vector2[] _pinOffsets = new Vector2[] {
            new(-8, -4), new(-8, 4), new(0, -6), new(0, 6), new(8, 0)
        };

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether the inputs need to be swapped.
        /// </summary>
        public bool SwapInputs
        {
            get => _swapInputs;
            set
            {
                _swapInputs = value;
                UpdatePins();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opamp"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Opamp(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, _pinOffsets[0], new(-1, 0)), "n", "neg");
            Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, _pinOffsets[1], new(-1, 0)), "p", "pos");
            Pins.Add(new FixedOrientedPin("negativesupply", "The negative supply.", this, _pinOffsets[2], new(0, 1)), "vn");
            Pins.Add(new FixedOrientedPin("positivesupply", "The positive supply.", this, _pinOffsets[3], new(0, -1)), "vp");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, _pinOffsets[4], new(1, 0)), "o", "out", "output");
            UpdatePins();
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[] {
                new Vector2(-8, -8),
                new Vector2(8, 0),
                new Vector2(-8, 8)
            });

            drawing.Segments(new Vector2[]
            {
                new(-6, -4), new(-4, -4),
            }.Select(v => _swapInputs ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), "minus");
            drawing.Segments(new Vector2[] {
                new(-5, 5), new(-5, 3),
                new(-6, 4), new(-4, 4)
            }.Select(v => _swapInputs ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), "plus");

            if (Pins["vn"].Connections > 0)
                drawing.Line(new(0, -4), new(0, -6));
            if (Pins["vp"].Connections > 0)
                drawing.Line(new(0, 4), new(0, 6));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(5, 5), new(1, 1));
        }

        protected void UpdatePins()
        {
            var pin1 = (FixedOrientedPin)Pins[0];
            var pin2 = (FixedOrientedPin)Pins[1];
            if (_swapInputs)
            {
                pin1.Offset = _pinOffsets[1];
                pin2.Offset = _pinOffsets[0];
            }
            else
            {
                pin1.Offset = _pinOffsets[0];
                pin2.Offset = _pinOffsets[1];
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Opamp {Name}";
    }
}
