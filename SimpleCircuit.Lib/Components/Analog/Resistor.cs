using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A resistor.
    /// </summary>
    [Drawable("R", "A resistor, potentially programmable.", "Analog")]
    public class ResistorFactory : DrawableFactory
    {
        private const string _programmable = "programmable";
        private const string _photoresistor = "photo";
        private const string _thermistor = "thermistor";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private double _wiper = 0.5, _length = 12;

            /// <inheritdoc/>
            [Description("The label next to the resistor.")]
            public string Label { get; set; }

            [Description("The label on the other side of the resistor.")]
            public string Label2 { get; set; }

            [Description("The length of the resistor.")]
            public double Length
            {
                get => _length;
                set
                {
                    _length = value;
                    UpdatePins(this, EventArgs.Empty);
                }
            }

            [Description("The number of zigs.")]
            public int Zigs { get; set; } = 3;

            [Description("The fraction of the wiper position.")]
            public double Wiper
            {
                get => _wiper;
                set
                {
                    _wiper = value;
                    UpdatePins(this, EventArgs.Empty);
                }
            }

            /// <inheritdoc />
            public override string Type => "resistor";

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("ctrl", "The controlling pin.", this, new(0, 4), new(0, 1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("n", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "neg", "b");
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                switch (Variants.Select(Options.Ansi, Options.Iec))
                {
                    case 1:
                        DrawIECResistor(drawing);
                        break;
                    case 0:
                    default:
                        DrawANSIResistor(drawing);
                        break;
                }

            }
            private void DrawANSIResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // The resistor
                List<Vector2> points = new();
                double increment = Length / Zigs * 0.5;
                double x = -Length / 2;
                points.Add(new(x, 0));
                x -= increment * 0.5;
                for (int i = 0; i < Zigs; i++)
                {
                    x += increment;
                    points.Add(new(x, -4));
                    x += increment;
                    points.Add(new(x, 4));
                }
                x += increment * 0.5;
                points.Add(new(x, 0));
                drawing.Polyline(points);

                switch (Variants.Select(_programmable, _photoresistor, _thermistor))
                {
                    case 0:
                        drawing.Arrow(new(-5, 5), new(6, -7));
                        drawing.Text(Label, new(0, -8), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 6), new(0, 1), new("lbl2"));
                        break;

                    case 1:
                        drawing.Arrow(new(-4, 9), new(-2, 5));
                        drawing.Arrow(new(0, 9), new(2, 5));
                        drawing.Text(Label, new(0, -5), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 10), new(0, 1), new("lbl2"));
                        break;

                    case 2:
                        drawing.Polyline(new Vector2[]
                        {
                            new(-8, 7), new(-4, 7), new(4, -7)
                        });
                        drawing.Text(Label, new(0, -8), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(-3, 7), new(1, 1), new("lbl2"));
                        break;

                    default:
                        drawing.Text(Label, new(0, -6), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 6), new(0, 1), new("lbl2"));
                        break;
                }
            }
            private void DrawIECResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // The rectangle
                drawing.Rectangle(Length, 6);

                switch (Variants.Select(_programmable, _photoresistor, _thermistor))
                {
                    case 0:
                        drawing.Arrow(new(-5, 4), new(6, -6));
                        drawing.Text(Label, new(0, -7), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 5), new(0, 1), new("lbl2"));
                        break;

                    case 1:
                        drawing.Arrow(new(-4, 8), new(-2, 4));
                        drawing.Arrow(new(0, 8), new(2, 4));
                        drawing.Text(Label, new(0, -4), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 9), new(0, 1), new("lbl2"));
                        break;

                    case 2:
                        drawing.Polyline(new Vector2[]
                        {
                            new(-8, 6), new(-4, 6), new(4, -6)
                        });
                        drawing.Text(Label, new(0, -7), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(-4, 6), new(1, 1), new("lbl2"));
                        break;

                    default:
                        drawing.Text(Label, new(0, -4), new(0, -1), new("lbl"));
                        drawing.Text(Label2, new(0, 4), new(0, 1), new("lbl2"));
                        break;
                }
            }

            private void UpdatePins(object sender, EventArgs e)
            {
                double x = Length * (_wiper - 0.5) * 0.5;
                if (Variants.Contains(Options.Iec))
                    SetPinOffset(1, new(x, 3));
                else
                    SetPinOffset(1, new(x, 4));

                x = Length * 0.5;
                SetPinOffset(0, new(-x, 0));
                SetPinOffset(-1, new(x, 0));
            }
        }
    }
}