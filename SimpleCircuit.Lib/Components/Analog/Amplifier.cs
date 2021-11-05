using SimpleCircuit.Components.Pins;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    [SimpleKey("A", "A generic amplifier.", Category = "Analog")]
    public class Amplifier : ScaledOrientedDrawable, ILabeled
    {
        private bool _differentialOutput = false, _differentialInput = false, _swapInputs = false, _swapOutputs = false;
        private static readonly Vector2[] _pinOffsets = new Vector2[] {
            new(-8, -4), new(-8, 4), new(8, -4), new(8, 4)
        };

        /// <inheritdoc/>
        [Description("The label in the amplifier.")]
        public string Label { get; set; }

        /// <summary>
        /// If <c>true</c>, the amplifier is displayed with a
        /// programmable gain (diagonal arrow).
        /// </summary>
        [Description("Displays a diagonal arrow.")]
        public bool Programmable { get; set; }

        /// <summary>
        /// Gets or sets whether a differential input is made.
        /// </summary>
        [Description("Splits the input pins into a differential input.")]
        public bool DifferentialInput
        {
            get => _differentialInput;
            set
            {
                _differentialInput = value;
                UpdatePins();
            }
        }

        /// <summary>
        /// Gets or sets whether a differential output is used.
        /// </summary>
        [Description("Splits the output pins into a differential output.")]
        public bool DifferentialOutput
        {
            get => _differentialOutput;
            set
            {
                _differentialOutput = value;
                UpdatePins();
            }
        }

        /// <summary>
        /// Gets or sets whether the inputs need to be swapped.
        /// </summary>
        [Description("Swaps positive and negative inputs.")]
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
        /// Gets or sets whether the outputs need to be swapped.
        /// </summary>
        [Description("Swaps positive and negative outputs.")]
        public bool SwapOutputs
        {
            get => _swapOutputs;
            set
            {
                _swapOutputs = value;
                UpdatePins();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Amplifier(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _pinOffsets[0], new(-1, 0)), "i", "in", "inp", "pi", "p");
            Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _pinOffsets[1], new(-1, 0)), "inn", "ni", "n");
            Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _pinOffsets[2], new(1, 0)), "outn", "no");
            Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _pinOffsets[3], new(1, 0)), "o", "out", "outp", "po");
            UpdatePins();
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new Vector2[]
            {
                new(-8, -8),
                new(8, 0),
                new(-8, 8)
            });

            // Draw plus and minus for the inputs
            if (_differentialInput)
            {
                drawing.Segments(new Vector2[]
                {
                    new(-6, -4), new(-4, -4),
                    new(-5, 5), new(-5, 3),
                }.Select(v => _swapInputs ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), "plus");
                drawing.Segments(new Vector2[] {
                    new(-6, 4), new(-4, 4)
                }.Select(v => _swapInputs ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), "minus");
            }

            if (_differentialOutput)
            {
                drawing.Segments(new Vector2[]
{
                    new(6, -6), new(4, -6),
                    new(5, 7), new(5, 5),
                }.Select(v => _swapInputs ? new Vector2(v.X, v.Y) : new Vector2(v.X, -v.Y)), "plus");
                drawing.Segments(new Vector2[] {
                    new(6, 6), new(4, 6)
                }.Select(v => _swapInputs ? new Vector2(v.X, -v.Y) : new Vector2(v.X, v.Y)), "minus");

                drawing.Segments(new Vector2[]
                {
                    new(0, 4), new(8, 4),
                    new(0, -4), new(8, -4)
                });
            }

            if (Programmable)
            {
                // Programmable gain amplifier
                drawing.Polyline(new Vector2[] {
                    new(-7, 10), new(4, -8.5),
                    new(4, -8.5), new(4, -6.5),
                    new(4, -8.5), new(2.25, -7.5)
                });
            }

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new(-2.5, 0), new(), 3, 0.5);
        }

        private void UpdatePins()
        {
            // Inputs
            var pin1 = (FixedOrientedPin)Pins[0];
            var pin2 = (FixedOrientedPin)Pins[1];
            if (_differentialInput)
            {
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
            else
            {
                var offset = (_pinOffsets[0] + _pinOffsets[1]) / 2.0;
                pin1.Offset = offset;
                pin2.Offset = offset;
            }

            // Outputs
            pin1 = (FixedOrientedPin)Pins[2];
            pin2 = (FixedOrientedPin)Pins[3];
            if (_differentialOutput)
            {
                if (_swapOutputs)
                {
                    pin1.Offset = _pinOffsets[3];
                    pin2.Offset = _pinOffsets[2];
                }
                else
                {
                    pin1.Offset = _pinOffsets[2];
                    pin2.Offset = _pinOffsets[3];
                }
            }
            else
            {
                var offset = (_pinOffsets[2] + _pinOffsets[3]) / 2.0;
                pin1.Offset = offset;
                pin2.Offset = offset;
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Amplifier {Name}";
    }
}
