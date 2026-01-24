using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// An inductor.
/// </summary>
[Drawable("L", "An inductor.", "Analog", "choke programmable", labelCount: 2)]
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
        private readonly CustomLabelAnchorPoints _anchors = new(2);
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

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
            AddPin(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
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
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = style.LineThickness * 0.5 + LabelMargin;
                    _anchors[0] = new LabelAnchorPoint(new(0, -4 - m), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 4 + m), new(0, 1));

                    // Let's clear the pins and re-add them correctly
                    Pins.Clear();

                    double l = Length * 0.5;
                    AddPin(new FixedOrientedPin("positive", "The positive pin.", this, new(-l, 0), new(-1, 0)), "p", "a");

                    // Add a tap for each winding
                    double x = -l;
                    switch (Variants.Select(Options.American, Options.European))
                    {
                        case 0:
                        case 1:
                            x += 3;
                            for (int i = 0; i < _windings - 1; i++)
                            {
                                AddPin(new FixedOrientedPin($"tap{i + 1}", $"Tap {i}", this, new(x, 0), new(0, 1)), $"tap{i + 1}", $"t{i + 1}");
                                x += 3;
                            }
                            break;

                        default:
                            x += 3;
                            for (int i = 0; i < _windings; i++)
                            {
                                AddPin(new FixedOrientedPin($"tap{i + 1}", $"Tap {i + 1}", this, new(x, 3), new(0, 1)), $"tap{i + 1}", $"t{i + 1}");
                                x += 3;
                            }
                            break;
                    }
                    AddPin(new FixedOrientedPin("negative", "The negative pin.", this, new(l, 0), new(1, 0)), "n", "b");
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            builder.ExtendPins(Pins, style, 2, "a", "b");
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
                    }, style.AsStroke());

                    if (Variants.Contains(_dot))
                    {
                        var marker = new Dot(new(-l, 3.5), new(1, 0));
                        marker.Draw(builder, style.AsLineThickness(Style.DefaultLineThickness));

                        double m = Style.DefaultLineThickness * 1.5 + LabelMargin;
                        if (_anchors[1].Location.Y < 3.5 + m)
                            _anchors[1] = new LabelAnchorPoint(new(0, 3.5 + m), new(0, 1));
                    }
                    else
                        _anchors[1] = new LabelAnchorPoint(new(0, style.LineThickness * 0.5 + LabelMargin), new(0, 1));

                    if (Variants.Contains(_choke))
                    {
                        builder.Line(new(-l, -4.5), new(l, -4.5), style);
                        double m = style.LineThickness * 0.5 + LabelMargin;
                        if (_anchors[0].Location.Y > -4.5 - m)
                            _anchors[0] = new LabelAnchorPoint(new(0, -4.5 - m), new(0, -1));
                        if (!Variants.Contains(_singleLine))
                        {
                            builder.Line(new(-l, -6), new(l, -6), style);
                            if (_anchors[0].Location.Y > -6 - m)
                                _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                        }
                        if (Variants.Contains(_programmable))
                        {
                            builder.Arrow(new(-l * 0.75, 1.5), new(l * 0.85, -10), style);
                            if (_anchors[0].Location.Y > -10 - m)
                                _anchors[0] = new LabelAnchorPoint(new(0, -10 - m), new(0, -1));
                            if (_anchors[1].Location.Y < 1.5 + m)
                                _anchors[1] = new LabelAnchorPoint(new(0, 1.5 + m), new(0, 1));
                        }
                    }
                    else if (Variants.Contains(_programmable))
                    {
                        builder.Arrow(new(-l * 0.75, 1.5), new(l * 0.85, -7), style);

                        double m = style.LineThickness * 0.5 + LabelMargin;
                        if (_anchors[0].Location.Y > -7 - m)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7 - m), new(0, -1));
                        if (_anchors[1].Location.Y < 1.5 + m)
                            _anchors[1] = new LabelAnchorPoint(new(0, 1.5 + m), new(0, 1));
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
                    }, style.AsStroke());

                    if (Variants.Contains(_dot))
                    {
                        var marker = new Dot(new(-l - 2, 3.5), new(1, 0));
                        marker.Draw(builder, style.AsLineThickness(Style.DefaultLineThickness));

                        double m = 1.5 * Style.DefaultLineThickness + LabelMargin;
                        if (_anchors[1].Location.Y < 3.5 + m)
                            _anchors[1] = new LabelAnchorPoint(new(0, 3.5 + m), new(0, 1));
                    }
                    else
                        _anchors[1] = new LabelAnchorPoint(new(0, 4 + style.LineThickness * 0.5 + LabelMargin), new(0, 1));

                    if (Variants.Contains(_choke))
                    {
                        builder.Line(new(-l, -4.5), new(l, -4.5), style);
                        if (!Variants.Contains(_singleLine))
                        {
                            builder.Line(new(-l, -6), new(l, -6), style);

                            double m = style.LineThickness * 0.5 + LabelMargin;
                            if (_anchors[0].Location.Y > -6 - m)
                                _anchors[0] = new LabelAnchorPoint(new(0, -6 - m), new(0, -1));
                        }
                        if (Variants.Contains(_programmable))
                        {
                            builder.Arrow(new(-l + 1, 5), new(l, -10), style);

                            double m = style.LineThickness * 0.5 + LabelMargin;
                            if (_anchors[0].Location.Y > -10 - m)
                                _anchors[0] = new LabelAnchorPoint(new(0, -10 - m), new(0, -1));
                            if (_anchors[1].Location.Y < 5 + m)
                                _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
                        }
                    }
                    else if (Variants.Contains(_programmable))
                    {
                        builder.Arrow(new(-l + 1, 5), new(l, -7), style);

                        double m = style.LineThickness * 0.5 + LabelMargin;
                        if (_anchors[0].Location.Y > -7 - m)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7 - m), new(0, -1));
                        if (_anchors[1].Location.Y < 5 + m)
                            _anchors[1] = new LabelAnchorPoint(new(0, 5 + m), new(0, 1));
                    }
                    break;
            }

            _anchors.Draw(builder, this, style);
        }
    }
}