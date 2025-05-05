using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A resistor.
    /// </summary>
    [Drawable("R", "A resistor, potentially programmable.", "Analog", "programmable photo photoresistor thermistor memristor")]
    public class ResistorFactory : DrawableFactory
    {
        private const string _programmable = "programmable";
        private const string _photoresistor = "photo";
        private const string _thermistor = "thermistor";
        private const string _x = "x";
        private const string _memristor = "memristor";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());
            private double _wiper = 0.5, _length = 12, _width = 8;
            private bool _isSet = false;

            [Description("The length of the resistor.")]
            [Alias("l")]
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
            [Alias("w")]
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
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
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

                        double w = Width * 0.5;
                        _anchors[0] = new LabelAnchorPoint(new(0, -w - 1), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, w + 1), new(0, 1), Appearance);
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 2, "a", "b");

                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanResistor(builder);
                        break;
                    case 0:
                    default:
                        DrawAmericanResistor(builder);
                        break;
                }
                _anchors.Draw(builder, Labels);
            }
            private void DrawAmericanResistor(IGraphicsBuilder builder)
            {
                double w = Width * 0.5;
                double l = Length * 0.5;

                // The resistor
                List<Vector2> points = [];
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
                builder.Polyline(points, Appearance);


                switch (Variants.Select(_programmable, _photoresistor, _thermistor))
                {
                    case 0:
                        builder.Arrow(new(-5, w + 1), new(6, -w - 2), Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -w - 3), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, w + 2), new(0, 1), Appearance);
                        break;

                    case 1:
                        builder.Arrow(new(-4, w + 5), new(-2, w + 1), Appearance);
                        builder.Arrow(new(0, w + 5), new(2, w + 1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, w + 6), new(0, 1), Appearance);
                        break;

                    case 2:
                        builder.Polyline(
                        [
                            new(-8, w + 3), new(-4, w + 3), new(4, -w - 3)
                        ], Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -w - 4), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(-3, w + 3), new(1, 1), Appearance);
                        break;
                }
            }
            private void DrawEuropeanResistor(IGraphicsBuilder builder)
            {
                double w = Width * 0.5;
                double l = Length * 0.5;

                // The rectangle
                builder.Rectangle(-Length * 0.5, -Width * 0.5, Length, Width, Appearance);
                if (Variants.Contains(_x))
                {
                    builder.Line(new(-l, -w), new(l, w), Appearance);
                    builder.Line(new(-l, w), new(l, -w), Appearance);
                }

                switch (Variants.Select(_programmable, _photoresistor, _thermistor, _memristor))
                {
                    case 0: // Programmable
                        builder.Arrow(new(-5, w + 1), new(6, -w - 3), Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -w - 3), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, w + 2), new(0, 1), Appearance);
                        break;

                    case 1: // Photoresistor
                        builder.Arrow(new(-4, w + 5), new(-2, w + 1), Appearance);
                        builder.Arrow(new(0, w + 5), new(2, w + 1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, w + 6), new(0, 1), Appearance);
                        break;

                    case 2: // Thermistor
                        builder.Polyline(
                        [
                            new(-l * 0.85, w + 2), new(-l * 0.85 + 2, w + 2), new(l * 0.85, -w - 2)
                        ], Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -w - 4), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(-l * 0.85 + 2, w + 2), new(1, 1), Appearance);
                        break;

                    case 3: // Memristor
                        builder.Rectangle(Length * 0.425 - Length * 0.15 * 0.5, -w, Length * 0.15, Width, Appearance);
                        double t = Length * 0.85 / 13;
                        builder.Polyline(
                        [
                            new(-l, 0), new(-l + 2 * t, 0), new(-l + 2 * t, -w * 0.5),
                            new(-l + 5 * t, -w * 0.5), new(-l + 5 * t, w * 0.5),
                            new(-l + 8 * t, w * 0.5), new(-l + 8 * t, -w * 0.5),
                            new(-l + 11 * t, -w * 0.5), new(-l + 11 * t, 0), new(-l + t * 13, 0)
                        ], Appearance);
                        break;
                }
            }
        }
    }
}