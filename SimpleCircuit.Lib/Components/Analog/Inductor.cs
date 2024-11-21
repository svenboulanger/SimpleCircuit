using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An inductor.
    /// </summary>
    [Drawable("L", "An inductor.", "Analog", "choke programmable")]
    public class Inductor : DrawableFactory
    {
        private const string _dot = "dot";
        private const string _programmable = "programmable";
        private const string _choke = "choke";
        private const string _singleLine = "single";
        
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());
            private int _windings = 3;

            [Description("The number of windings.")]
            public int Windings
            {
                get => _windings;
                set
                {
                    if (value < 0)
                        _windings = 1;
                    else
                        _windings = value;
                }
            }

            /// <summary>
            /// Gets the length of the inductor.
            /// </summary>
            public double Length
            {
                get
                {
                    return Variants.Select(Options.American, Options.European) switch
                    {
                        0 or 1 => _windings * 3,
                        _ => (double)(6 + (_windings - 1) * 3),
                    };
                }
            }

            /// <inheritdoc />
            public override string Type => "inductor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));

                // Let's clear the pins and re-add them correctly
                Pins.Clear();

                double l = Length * 0.5;
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-l, 0), new(-1, 0)), "p", "a");

                // Add a tap for each winding
                double x = -l;
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 0:
                    case 1:
                        x += 3;
                        for (int i = 0; i < _windings - 1; i++)
                        {
                            Pins.Add(new FixedOrientedPin($"tap{i + 1}", $"Tap {i}", this, new(x, 0), new(0, 1)), $"tap{i + 1}", $"t{i + 1}");
                            x += 3;
                        }
                        break;

                    default:
                        x += 3;
                        for (int i = 0; i < _windings; i++)
                        {
                            Pins.Add(new FixedOrientedPin($"tap{i + 1}", $"Tap {i + 1}", this, new(x, 3), new(0, 1)), $"tap{i + 1}", $"t{i + 1}");
                            x += 3;
                        }
                        break;
                }
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(l, 0), new(1, 0)), "n", "b");
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, 2, "a", "b");
                double l = Length * 0.5;
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 0:
                    case 1:
                        builder.Path(b =>
                        {
                            double x = -l;
                            b.MoveTo(new(x, 0));
                            for (int i = 0; i < Windings; i++)
                            {
                                b.CurveTo(new(x, -4), new(x + 3, -4), new(x + 3, 0));
                                x += 3;
                            }
                        });

                        if (Variants.Contains(_dot))
                        {
                            var marker = new Dot(new(-l, 3.5), new(1, 0));
                            marker.Draw(builder);
                        }
                        
                        if (Variants.Contains(_choke))
                        {
                            builder.Line(new(-l, -4.5), new(l, -4.5), new("choke"));
                            if (_anchors[0].Location.Y > -5.5)
                                _anchors[0] = new LabelAnchorPoint(new(0, -5.5), new(0, -1));
                            if (!Variants.Contains(_singleLine))
                            {
                                builder.Line(new(-l, -6), new(l, -6), new("choke"));
                                if (_anchors[0].Location.Y > -7)
                                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                            }
                            if (Variants.Contains(_programmable))
                            {
                                builder.Arrow(new(-l * 0.75, 1.5), new(l * 0.85, -10));
                                if (_anchors[0].Location.Y > -11)
                                    _anchors[0] = new LabelAnchorPoint(new(0, -11), new(0, -1));
                            }
                        }
                        else if (Variants.Contains(_programmable))
                        {
                            builder.Arrow(new(-l * 0.75, 1.5), new(l * 0.85, -7));
                            if (_anchors[0].Location.Y > -8)
                                _anchors[0] = new LabelAnchorPoint(new(0, -8), new(0, -1));
                        }
                        break;

                    default:
                        builder.Path(b =>
                        {
                            double x = -l;
                            b.MoveTo(new(x, 0));
                            b.CurveTo(new(x, -4), new(x + 4, -4), new(x + 4, 0));
                            x += 2;
                            b.SmoothTo(new(x, 4), new(x, 0));
                            for (int i = 0; i < Windings - 1; i++)
                            {
                                x += 5;
                                b.SmoothTo(new(x, -4), new(x, 0));
                                x -= 2;
                                b.SmoothTo(new(x, 4), new(x, 0));
                            }
                            x += 4;
                            b.SmoothTo(new(x, -4), new(x, 0));
                        });

                        if (Variants.Contains(_dot))
                        {
                            var marker = new Dot(new(-l - 2, 3.5), new(1, 0));
                            marker.Draw(builder);
                        }
                        
                        if (Variants.Contains(_choke))
                        {
                            builder.Line(new(-l, -4.5), new(l, -4.5), new("choke"));
                            if (!Variants.Contains(_singleLine))
                            {
                                builder.Line(new(-l, -6), new(l, -6), new("choke"));
                                if (_anchors[0].Location.Y > -7)
                                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                            }
                            if (Variants.Contains(_programmable))
                            {
                                builder.Arrow(new(-l + 1, 5), new(l, -10));
                                if (_anchors[0].Location.Y > -11)
                                    _anchors[0] = new LabelAnchorPoint(new(0, -11), new(0, -1));
                            }
                        }
                        else if (Variants.Contains(_programmable))
                        {
                            builder.Arrow(new(-l + 1, 5), new(l, -7));
                            if (_anchors[0].Location.Y > -8)
                                _anchors[0] = new LabelAnchorPoint(new(0, -8), new(0, -1));
                        }
                        break;
                }

                _anchors.Draw(builder, Labels);
            }
        }
    }
}