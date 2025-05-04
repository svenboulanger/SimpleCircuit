using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    [Drawable("S", "A switch. The controlling pin is optional.", "Analog", "push lamp window toggle knife reed arei")]
    public class Switch : DrawableFactory
    {
        private const string _closed = "closed";
        private const string _invert = "invert";
        private const string _push = "push";
        private const string _lamp = "lamp";
        private const string _window = "window";
        private const string _toggle = "toggle";
        private const string _double = "double";
        private const string _knife = "knife";
        private const string _closing = "closing";
        private const string _opening = "opening";
        private const string _reed = "reed";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IStandardizedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            [Description("The number of poles on the switch.")]
            [Alias("p")]
            public int Poles { get; set; }

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.AREI;

            /// <inheritdoc />
            public override string Type => "switch";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, -6), new(0, -1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("backside", "The backside controlling pin. Can be used to link multiple switches.", this, new(0, -6), new(0, 1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");
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
                        switch (Variants.Select(Options.Arei))
                        {
                            case 0:
                                if (Variants.Contains(_push))
                                {
                                    SetPinOffset(0, new(-4, 0));
                                    SetPinOffset(3, new(4, 0));
                                }
                                else
                                {
                                    SetPinOffset(0, new(-2, 0));
                                    SetPinOffset(3, new(2, 0));
                                }
                                break;

                            default:
                                SetPinOffset(0, new(-6, 0));
                                SetPinOffset(3, new(6, 0));

                                if (Variants.Contains(_invert))
                                {
                                    if (Variants.Contains(_closed))
                                    {
                                        SetPinOffset(1, new(0, -2));
                                        SetPinOffset(2, new());
                                    }
                                    else
                                    {
                                        SetPinOffset(1, new(0, -4.25));
                                        SetPinOffset(2, new(0, -2));
                                    }
                                }
                                else
                                {
                                    if (Variants.Contains(_closed))
                                    {
                                        SetPinOffset(1, new());
                                        SetPinOffset(2, new());
                                    }
                                    else
                                    {
                                        SetPinOffset(1, new(0, -2));
                                        SetPinOffset(2, new(0, -2));
                                    }
                                }
                                break;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                switch (Variants.Select(Options.Arei, Options.American))
                {
                    case 0: DrawAreiSwitch(builder); break;
                    case 1:
                    default: DrawSwitch(builder); break;
                }

                _anchors.Draw(builder, Labels);
            }

            private void DrawSwitch(IGraphicsBuilder builder)
            {
                if (Variants.Contains(_knife))
                    DrawKnifeSwitch(builder);
                else if (Variants.Contains(_push))
                    DrawPushSwitch(builder);
                else
                    DrawRegularSwitch(builder);
            }
            private void DrawKnifeSwitch(IGraphicsBuilder builder)
            {
                if (Variants.Contains(_closed))
                {
                    builder.Circle(new(-5, 0), 1, Appearance);
                    builder.Circle(new(5, 0), 1, Appearance);
                    builder.Line(new(-4, 0), new(4, 0), Appearance);
                    builder.Line(new(0, 2), new(0, -2), Appearance);
                }
                else
                {
                    builder.Polyline([new(-6, 0), new(-4, 0), new(2, -4)], Appearance);
                    builder.Line(new(4, 0), new(6, 0), Appearance);
                    builder.Line(new(-0.5, -4), new(1.5, -1.5), Appearance);
                }

                _anchors[0] = new LabelAnchorPoint(new(3, -3), new(1, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1), Appearance);
            }
            private void DrawRegularSwitch(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 2, "a", "b");

                // Switch terminals
                builder.Circle(new Vector2(-5, 0), 1, Appearance);
                builder.Circle(new Vector2(5, 0), 1, Appearance);
                _anchors[0] = new LabelAnchorPoint(new(0, -1.5), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 1.5), new(0, 1), Appearance);

                if (Variants.Contains(_closed))
                {
                    if (Variants.Contains(_invert))
                    {
                        builder.Circle(new(0, -1), 1, Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -2.5), new(0, -1), Appearance);
                    }
                    builder.Line(new(-4, 0), new(4, 0), Appearance);
                }
                else
                {
                    if (Variants.Contains(_invert))
                        builder.Circle(new(0, -3.25), 1, Appearance);
                    builder.Line(new(-4, 0), new(4, -4), Appearance);
                    _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                }

                switch (Variants.Select(_closing, _opening))
                {
                    case 0:
                        builder.Path(b => b
                            .MoveTo(new(-3, -5))
                            .CurveTo(new(1, -3), new(1, -2), new(2, 2)), Appearance);
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                        if (_anchors[1].Location.Y < 3)
                            _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1), Appearance);
                        var marker = new Arrow(new(2, 2), new(0.24253562503, 0.97014250014));
                        marker.Draw(builder, Appearance);
                        break;

                    case 1:
                        builder.Path(b => b
                            .MoveTo(new(-4, -6))
                            .CurveTo(new(1, -3), new(1, -2), new(2, 1)), Appearance);
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1), Appearance);
                        if (_anchors[1].Location.Y < 2)
                            _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1), Appearance);
                        marker = new Arrow(new(-4, -6), new(-0.80873608430318844, -0.58817169767504618));
                        marker.Draw(builder, Appearance);
                        break;
                }

                if (Variants.Contains(_reed))
                {
                    builder.Path(b =>
                    {
                        b.MoveTo(new(-5, -6));
                        b.LineTo(new(5, -6));
                        b.CurveTo(new(8.3, -6), new(11, -3.3), new(11, 0));
                        b.SmoothTo(new(8.3, 6), new(5, 6));
                        b.LineTo(new(-5, 6));
                        b.CurveTo(new(-8.3, 6), new(-11, 3.3), new(-11, 0));
                        b.SmoothTo(new(-8.3, -6), new(-5, -6));
                        b.Close();
                    }, Appearance);
                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1), Appearance);
                    _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1), Appearance);
                }
            }
            private void DrawPushSwitch(IGraphicsBuilder builder)
            {
                // Switch terminals
                builder.Circle(new Vector2(-5, 0), 1, Appearance);
                builder.Circle(new Vector2(5, 0), 1, Appearance);

                if (Variants.Contains(_closed))
                {
                    builder.Line(new(-4, 0), new(4, 0), Appearance);
                    if (Variants.Contains(_invert))
                    {
                        builder.Circle(new(0, -1), 1, Appearance);
                        builder.Line(new(0, -2), new(0, -6), Appearance);
                    }
                    else
                        builder.Line(new(0, 0), new(0, -6), Appearance);
                }
                else
                {
                    builder.Line(new(-5, -4), new(5, -4), Appearance);
                    if (Variants.Contains(_invert))
                    {
                        builder.Circle(new(0, -5), 1, Appearance);
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                    }
                    else
                    {
                        builder.Line(new(0, -4), new(0, -6), Appearance);
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1), Appearance);
                    }
                }

                if (_anchors[1].Location.Y < 2)
                    _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1), Appearance);
            }

            private void DrawAreiSwitch(IGraphicsBuilder builder)
            {
                if (Variants.Contains(_push))
                    DrawAreiPushSwitch(builder);
                else
                    DrawAreiRegularSwitch(builder);
            }
            private void DrawAreiPushSwitch(IGraphicsBuilder builder)
            {
                builder.ExtendPin(Pins["a"], Appearance);
                builder.Circle(new(), 4, Appearance);
                builder.Circle(new(), 2, Appearance);

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance);

                if (Variants.Contains(_lamp))
                {
                    double x = 2 / Math.Sqrt(2);
                    builder.Path(b => b.MoveTo(new(-x, -x)).LineTo(new(x, x)).MoveTo(new(-x, x)).LineTo(new(x, -x)), Appearance);
                }

                if (Variants.Contains(_window))
                {
                    builder.Polyline([
                        new(-6.5, 2.5),
                        new(-4.5, 2.5),
                        new(-4.5, 6),
                        new(4.5, 6),
                        new(4.5, 2.5),
                        new(6.5, 2.5)
                    ], Appearance);
                    if (_anchors[1].Location.Y < 7)
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1), Appearance);
                }
            }
            private void DrawAreiRegularSwitch(IGraphicsBuilder builder)
            {
                double length = Math.Max(8, 5 + Math.Max(1, Poles) * 2);
                builder.Circle(new(), 2, Appearance);
                var n = Vector2.Normal(-Math.PI * 0.37);
                builder.Line(n * 2, n * length, Appearance);
                if (Variants.Contains(_toggle))
                    builder.Line(-n * 2, -n * length, Appearance);
                if (Variants.Contains(_double))
                {
                    Vector2 np = new(-n.X, n.Y);
                    builder.Line(np * 2, np * length, Appearance);
                    if (Variants.Contains(_toggle))
                        builder.Line(-np * 2, -np * length, Appearance);
                }

                // Label
                _anchors[0] = new LabelAnchorPoint(new(0, -length), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, length), new(0, 1), Appearance);

                // Small cross for illuminator lamps
                if (Variants.Contains(_lamp))
                {
                    double x = 2.0 / Math.Sqrt(2.0);
                    builder.Path(b => b.MoveTo(new(-x, -x)).LineTo(new(x, x)).MoveTo(new(-x, x)).LineTo(new(x, -x)), Appearance);
                }

                // Draw the poles
                if (Poles > 0)
                {
                    var p = n.Perpendicular;
                    builder.Path(b =>
                    {
                        var pts = new List<Vector2>(Poles * 2);
                        for (int i = 0; i < Poles; i++)
                        {
                            // Draw from out to in
                            var r = n * length;
                            var end = r + p * 3;
                            b.MoveTo(r).LineTo(end);
                            if (Variants.Contains(_toggle))
                                b.MoveTo(-r).LineTo(-end);
                            if (Variants.Contains(_double))
                            {
                                b.MoveTo(new(-r.X, r.Y)).LineTo(new(-end.X, end.Y));
                                if (Variants.Contains(_toggle))
                                    b.MoveTo(new(r.X, -r.Y)).LineTo(new(end.X, -end.Y));
                            }
                            length -= 2.0;
                        }
                    }, Appearance);
                }
            }
        }
    }
}