using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Markers;
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

        private class Instance : ScaledOrientedDrawable, ILabeled, IStandardizedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            [Description("The number of poles on the switch.")]
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
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

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
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.Arei, Options.American))
                {
                    case 0: DrawAreiSwitch(drawing); break;
                    case 1:
                    default: DrawSwitch(drawing); break;
                }

                _anchors.Draw(drawing, this);
            }

            private void DrawSwitch(SvgDrawing drawing)
            {
                if (Variants.Contains(_knife))
                    DrawKnifeSwitch(drawing);
                else if (Variants.Contains(_push))
                    DrawPushSwitch(drawing);
                else
                    DrawRegularSwitch(drawing);
            }
            private void DrawKnifeSwitch(SvgDrawing drawing)
            {
                if (Variants.Contains(_closed))
                {
                    drawing.Circle(new(-5, 0), 1);
                    drawing.Circle(new(5, 0), 1);
                    drawing.Line(new(-4, 0), new(4, 0), new("wire"));
                    drawing.Line(new(0, 2), new(0, -2));
                }
                else
                {
                    drawing.Polyline(new Vector2[] { new(-6, 0), new(-4, 0), new(2, -4) }, new("wire"));
                    drawing.Line(new(4, 0), new(6, 0), new("wire"));
                    drawing.Line(new(-0.5, -4), new(1.5, -1.5));
                }

                _anchors[0] = new LabelAnchorPoint(new(3, -3), new(1, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1));
            }
            private void DrawRegularSwitch(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // Switch terminals
                drawing.Circle(new Vector2(-5, 0), 1);
                drawing.Circle(new Vector2(5, 0), 1);
                _anchors[0] = new LabelAnchorPoint(new(0, -1.5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 1.5), new(0, 1));

                if (Variants.Contains(_closed))
                {
                    if (Variants.Contains(_invert))
                    {
                        drawing.Circle(new(0, -1), 1);
                        _anchors[0] = new LabelAnchorPoint(new(0, -2.5), new(0, -1));
                    }
                    drawing.Line(new(-4, 0), new(4, 0), new("wire"));
                }
                else
                {
                    if (Variants.Contains(_invert))
                        drawing.Circle(new(0, -3.25), 1);
                    drawing.Line(new(-4, 0), new(4, -4), new("wire"));
                    _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                }

                switch (Variants.Select(_closing, _opening))
                {
                    case 0:
                        drawing.OpenBezier(new Vector2[]
                        {
                            new(-3, -5), new(1, -3), new(1, -2), new(2, 2)
                        });
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                        if (_anchors[1].Location.Y < 3)
                            _anchors[1] = new LabelAnchorPoint(new(0, 3), new(0, 1));
                        var marker = new Arrow(new(2, 2), new(0.24253562503, 0.97014250014));
                        marker.Draw(drawing);
                        break;

                    case 1:
                        drawing.OpenBezier(new Vector2[]
                        {
                           new(-4, -6), new(1, -3), new(1, -2), new(2, 1)
                        });
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                        if (_anchors[1].Location.Y < 2)
                            _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
                        marker = new Arrow(new(-4, -6), new(-0.80873608430318844, -0.58817169767504618));
                        marker.Draw(drawing);
                        break;
                }

                if (Variants.Contains(_reed))
                {
                    drawing.Path(b =>
                    {
                        b.MoveTo(-5, -6);
                        b.LineTo(5, -6);
                        b.CurveTo(new(8.3, -6), new(11, -3.3), new(11, 0));
                        b.SmoothTo(new(8.3, 6), new(5, 6));
                        b.LineTo(-5, 6);
                        b.CurveTo(new(-8.3, 6), new(-11, 3.3), new(-11, 0));
                        b.SmoothTo(new(-8.3, -6), new(-5, -6));
                        b.Close();
                    });
                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                }
            }
            private void DrawPushSwitch(SvgDrawing drawing)
            {
                // Switch terminals
                drawing.Circle(new Vector2(-5, 0), 1);
                drawing.Circle(new Vector2(5, 0), 1);

                if (Variants.Contains(_closed))
                {
                    drawing.Line(new(-4, 0), new(4, 0));
                    if (Variants.Contains(_invert))
                    {
                        drawing.Circle(new(0, -1), 1);
                        drawing.Line(new(0, -2), new(0, -6), new("wire"));
                    }
                    else
                        drawing.Line(new(0, 0), new(0, -6), new("wire"));
                }
                else
                {
                    drawing.Line(new(-5, -4), new(5, -4));
                    if (Variants.Contains(_invert))
                    {
                        drawing.Circle(new(0, -5), 1);
                        if (_anchors[0].Location.Y > -6)
                            _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                    }
                    else
                    {
                        drawing.Line(new(0, -4), new(0, -6), new("wire"));
                        if (_anchors[0].Location.Y > -7)
                            _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    }
                }

                if (_anchors[1].Location.Y < 2)
                    _anchors[1] = new LabelAnchorPoint(new(0, 2), new(0, 1));
            }

            private void DrawAreiSwitch(SvgDrawing drawing)
            {
                if (Variants.Contains(_push))
                    DrawAreiPushSwitch(drawing);
                else
                    DrawAreiRegularSwitch(drawing);
            }
            private void DrawAreiPushSwitch(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["a"]);
                drawing.Circle(new(), 4);
                drawing.Circle(new(), 2);

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));

                if (Variants.Contains(_lamp))
                {
                    double x = 2 / Math.Sqrt(2);
                    drawing.Path(b => b.MoveTo(-x, -x).LineTo(x, x).MoveTo(-x, x).LineTo(x, -x), new("lamp"));
                }

                if (Variants.Contains(_window))
                {
                    drawing.Polyline(new Vector2[]
                    {
                        new(-6.5, 2.5), new(-4.5, 2.5), new(-4.5, 6), new(4.5, 6), new(4.5, 2.5), new(6.5, 2.5)
                    }, new("window"));
                    if (_anchors[1].Location.Y < 7)
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                }
            }
            private void DrawAreiRegularSwitch(SvgDrawing drawing)
            {
                double length = Math.Max(8, 5 + Math.Max(1, Poles) * 2);
                drawing.Circle(new(), 2);
                var n = Vector2.Normal(-Math.PI * 0.37);
                drawing.Line(n * 2, n * length);
                if (Variants.Contains(_toggle))
                    drawing.Line(-n * 2, -n * length);
                if (Variants.Contains(_double))
                {
                    Vector2 np = new(-n.X, n.Y);
                    drawing.Line(np * 2, np * length);
                    if (Variants.Contains(_toggle))
                        drawing.Line(-np * 2, -np * length);
                }

                // Label
                _anchors[0] = new LabelAnchorPoint(new(0, -length), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, length), new(0, 1));

                // Small cross for illuminator lamps
                if (Variants.Contains(_lamp))
                {
                    double x = 2.0 / Math.Sqrt(2.0);
                    drawing.Path(b => b.MoveTo(-x, -x).LineTo(x, x).MoveTo(-x, x).LineTo(x, -x), new("lamp"));
                }

                // Draw the poles
                if (Poles > 0)
                {
                    var p = n.Perpendicular;
                    drawing.Path(b =>
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
                                b.MoveTo(-r.X, r.Y).LineTo(-end.X, end.Y);
                                if (Variants.Contains(_toggle))
                                    b.MoveTo(r.X, -r.Y).LineTo(end.X, -end.Y);
                            }
                            length -= 2.0;
                        }
                    });
                }
            }
        }
    }
}