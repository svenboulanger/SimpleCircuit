using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
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
        private const string _x = "x";
        private const string _assymmetric = "assymmetric";
        private const string _memristor = "memristor";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private double _wiper = 0.5, _length = 12, _width = 8;
            private bool _isSet = false;
            private readonly Vector2[] _locations = new Vector2[2];
            private readonly Vector2[] _expands = new Vector2[] { new(0, -1), new(0, 1) };

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The length of the resistor.")]
            public double Length
            {
                get => _length;
                set
                {
                    _length = value;
                    if (_length < 4)
                        _length = 4;
                }
            }

            [Description("The width of the resistor.")]
            public double Width
            {
                get => _width;
                set
                {
                    _width = value;
                    _isSet = true;
                    if (_width < 4)
                        _width = 4;
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
                    if (_wiper < 0)
                        _wiper = 0;
                    if (_wiper > 1)
                        _wiper = 1;
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
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
                SetPinOffset(Pins["p"], new(-_length * 0.5, 0));
                SetPinOffset(Pins["n"], new(_length * 0.5, 0));

                double x = Length * (_wiper - 0.5);
                SetPinOffset(Pins["c"], new(x, _width * 0.5));

                if (!_isSet)
                {
                    if (Variants.Contains(Options.European))
                        _width = 6;
                    else
                        _width = 8;
                }
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                _locations[0] = default;
                _locations[1] = default;
                _expands[0] = new(0, -1);
                _expands[1] = new(0, 1);
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanResistor(drawing);
                        break;
                    case 0:
                    default:
                        DrawAmericanResistor(drawing);
                        break;
                }
                Labels.SetDefaultPin(0, location: _locations[0], expand: _expands[0]);
                Labels.SetDefaultPin(1, location: _locations[1], expand: _expands[1]);
                Labels.Draw(drawing);
            }
            private void DrawAmericanResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                double w = Width * 0.5;
                double l = Length * 0.5;

                // The resistor
                List<Vector2> points = new();
                double increment = Length / Zigs * 0.5;
                double x = -l;
                points.Add(new(x, 0));
                x -= increment * 0.5;
                for (int i = 0; i < Zigs; i++)
                {
                    x += increment;
                    points.Add(new(x, -w));
                    x += increment;
                    points.Add(new(x, w));
                }
                x += increment * 0.5;
                points.Add(new(x, 0));
                drawing.Polyline(points);


                switch (Variants.Select(_programmable, _photoresistor, _thermistor))
                {
                    case 0:
                        drawing.Arrow(new(-5, w + 1), new(6, -w - 2));
                        _locations[0].ExpandUp(-w - 3);
                        _locations[1].ExpandDown(w + 2);
                        break;

                    case 1:
                        drawing.Arrow(new(-4, w + 5), new(-2, w + 1));
                        drawing.Arrow(new(0, w + 5), new(2, w + 1));
                        _locations[0].ExpandUp(-w - 1);
                        _locations[1].ExpandDown(w + 6);
                        break;

                    case 2:
                        drawing.Polyline(new Vector2[]
                        {
                            new(-8, w + 3), new(-4, w + 3), new(4, -w - 3)
                        });
                        _locations[0].ExpandUp(-w - 4);
                        _locations[1] = new Vector2(-3, w + 3);
                        _expands[1] = new Vector2(1, 1);
                        break;

                    default:
                        _locations[0].ExpandUp(-w - 2);
                        _locations[1].ExpandDown(w + 2);
                        break;
                }
            }
            private void DrawEuropeanResistor(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");
                double w = Width * 0.5;
                double l = Length * 0.5;

                // The rectangle
                drawing.Rectangle(-Length * 0.5, -Width * 0.5, Length, Width);
                if (Variants.Contains(_x))
                {
                    drawing.Line(new(-l, -w), new(l, w));
                    drawing.Line(new(-l, w), new(l, -w));
                }

                switch (Variants.Select(_programmable, _photoresistor, _thermistor, _assymmetric, _memristor))
                {
                    case 0: // Programmable
                        drawing.Arrow(new(-5, w + 1), new(6, -w - 3));
                        _locations[0].ExpandUp(-w - 3);
                        _locations[1].ExpandDown(w + 2);
                        break;

                    case 1: // Photoresistor
                        drawing.Arrow(new(-4, w + 5), new(-2, w + 1));
                        drawing.Arrow(new(0, w + 5), new(2, w + 1));
                        _locations[0].ExpandUp(-w - 1);
                        _locations[1].ExpandDown(w + 6);
                        break;

                    case 2: // Thermistor
                        drawing.Polyline(new Vector2[]
                        {
                            new(-l * 0.85, w + 2), new(-l * 0.85 + 2, w + 2), new(l * 0.85, -w - 2)
                        });
                        _locations[0].ExpandUp(-w - 4);
                        _locations[1] = new Vector2(-l * 0.85 + 2, w + 2);
                        _expands[1] = new Vector2(1, 1);
                        break;

                    case 3: // Assymmetric
                        drawing.Rectangle(l * 0.85 - Width * 0.15 * 0.5, -w,
                            l * 0.85 + Width * 0.15 * 0.5, w, options: new("marker"));
                        _locations[0].ExpandUp(-w - 1);
                        _locations[1].ExpandDown(w + 1);
                        break;

                    case 4: // Memristor
                        drawing.Rectangle(Length * 0.425 - Length * 0.15 * 0.5, -w, Length * 0.15, Width, options: new("marker"));
                        double t = Length * 0.85 / 13;
                        drawing.Polyline(new Vector2[]
                        {
                            new(-l, 0), new(-l + 2 * t, 0), new(-l + 2 * t, -w * 0.5),
                            new(-l + 5 * t, -w * 0.5), new(-l + 5 * t, w * 0.5),
                            new(-l + 8 * t, w * 0.5), new(-l + 8 * t, -w * 0.5),
                            new(-l + 11 * t, -w * 0.5), new(-l + 11 * t, 0), new(-l + t * 13, 0)
                        });
                        _locations[0].ExpandUp(-w - 1);
                        _locations[1].ExpandDown(w + 1);
                        break;

                    default:
                        _locations[0].ExpandUp(-4);
                        _locations[1].ExpandDown(4);
                        break;
                }
            }
        }
    }
}