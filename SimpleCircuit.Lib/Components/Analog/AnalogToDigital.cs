using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// ADC.
    /// </summary>
    [SimpleKey("ADC", "An Analog-to-digital converter.", Category = "Digital")]
    public class AnalogToDigital : ScaledOrientedDrawable, ILabeled
    {
        private double _width = 18, _height = 12;
        private bool _differentialOutput = false;
        private bool _differentialInput = false;

        /// <summary>
        /// Gets or sets the width of the ADC.
        /// </summary>
        [Description("The width of the ADC.")]
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                UpdatePins();
            }
        }

        /// <summary>
        /// Gets or sets the height of the ADC.
        /// </summary>
        [Description("The height of the ADC.")]
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                UpdatePins();
            }
        }

        /// <inheritdoc/>
        [Description("The label inside the ADC.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether a differential input is made.
        /// </summary>
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
        public bool DifferentialOutput
        {
            get => _differentialOutput;
            set
            {
                _differentialOutput = value;
                UpdatePins();
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GroupClasses
        {
            get
            {
                if (DifferentialInput)
                    yield return "diffin";
                if (DifferentialOutput)
                    yield return "diffout";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogToDigital"/>
        /// </summary>
        /// <param name="name">The name of the ADC.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public AnalogToDigital(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, new(-9, 0), new(-1, 0)), "input", "in", "pi", "inp");
            Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, new(-9, 0), new(-1, 0)), "inn", "ni");
            Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(9, 0), new(1, 0)), "outn", "no");
            Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, new(9, 0), new(1, 0)), "output", "out", "po", "outp");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-_width / 2, _height / 2), new Vector2(_width / 2 - _height / 2, _height / 2),
                new Vector2(_width / 2, 0), new Vector2(_width / 2 - _height / 2, -_height / 2),
                new Vector2(-_width / 2, -_height / 2)
            });

            if (_differentialOutput)
            {
                drawing.Segments(new[]
                {
                    new Vector2(_width / 2 - _height / 4, _height / 4), new Vector2(_width / 2, _height / 4),
                    new Vector2(_width / 2 - _height / 4, -_height / 4), new Vector2(_width / 2, -_height / 4)
                }, new("wire"));
            }

            if (!string.IsNullOrWhiteSpace(Label))
            {
                drawing.Text(Label, new Vector2(-_height / 4, 0), new Vector2(0, 0));
            }
        }

        private void UpdatePins()
        {
            var pin1 = (FixedOrientedPin)Pins[0];
            var pin2 = (FixedOrientedPin)Pins[1];
            if (_differentialInput)
            {
                pin1.Offset = new(-_width / 2, -_height / 4);
                pin2.Offset = new(-_width / 2, _height / 4);
            }
            else
            {
                pin1.Offset = new(-_width / 2, 0);
                pin2.Offset = new(-_width / 2, 0);
            }

            pin1 = (FixedOrientedPin)Pins[2];
            pin2 = (FixedOrientedPin)Pins[3];
            if (_differentialOutput)
            {
                pin1.Offset = new(_width / 2 - _height / 4, _height / 4);
                pin2.Offset = new(_width / 2 - _height / 4, -_height / 4);
            }
            else
            {
                pin1.Offset = new(_width / 2, 0);
                pin2.Offset = new(_width / 2, 0);
            }
        }

        /// <summary>
        /// Converts to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"ADC {Name}";
    }
}
