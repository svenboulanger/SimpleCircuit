using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Linq;
using static SimpleCircuit.Components.Variant;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A switch.
    /// </summary>
    [Drawable("S", "A switch. The controlling pin is optional.", "Analog")]
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

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            var device = new Instance(name, options);
            switch (options?.Style ?? Options.Styles.ANSI)
            {
                case Options.Styles.AREI:
                    device.AddVariant(Options.Arei);
                    break;
                case Options.Styles.IEC:
                    device.AddVariant(Options.Iec);
                    break;
                default:
                    device.AddVariant(Options.Ansi);
                    break;
            }
            return device;
        }

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the switch.")]
            public string Label { get; set; }
            [Description("The number of poles on the switch.")]
            public int Poles { get; set; }

            /// <inheritdoc />
            public override string Type => "switch";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "p", "a");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, -6), new(0, -1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("backside", "The backside controlling pin. Can be used to link multiple switches.", this, new(0, -6), new(0, 1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b");

                PinUpdate = All(
                    Map(Options.Arei, _push, UpdatePins),
                    IfNot(Options.Arei).Then(Map(_closed, _invert, UpdateControlPin)));
                DrawingVariants = If(Options.Arei)
                    .Then(
                        If(_push)
                        .Then(Map(_lamp, _window, DrawAreiPushSwitch))
                        .Else(Map(_toggle, _double, _lamp, DrawAreiSwitch)))
                    .Else(
                        If(_knife).Then(Map(_closed, DrawKnifeSwitch)).Else(
                        If(_push)
                        .Then(Map(_closed, _invert, DrawPushSwitch))
                        .Else(Map(_closed, _invert, DrawRegularSwitch))));
            }

            private void DrawKnifeSwitch(SvgDrawing drawing, bool closed)
            {
                if (closed)
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

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }
            private void DrawRegularSwitch(SvgDrawing drawing, bool closed, bool inverted)
            {
                drawing.ExtendPins(Pins, 2, "a", "b");

                // Switch terminals
                drawing.Circle(new Vector2(-5, 0), 1);
                drawing.Circle(new Vector2(5, 0), 1);

                if (closed)
                {
                    if (inverted)
                        drawing.Circle(new(0, -1), 1);
                    drawing.Line(new(-4, 0), new(4, 0), new("wire"));
                }
                else
                {
                    if (inverted)
                        drawing.Circle(new(0, -3.25), 1);
                    drawing.Line(new(-4, 0), new(4, -4), new("wire"));
                }

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, 3), new Vector2(0, 1));
            }
            private void DrawPushSwitch(SvgDrawing drawing, bool closed, bool inverted)
            {
                // Switch terminals
                drawing.Circle(new Vector2(-5, 0), 1);
                drawing.Circle(new Vector2(5, 0), 1);

                if (closed)
                {
                    drawing.Line(new(-4, 0), new(4, 0));
                    if (inverted)
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
                    if (inverted)
                    {
                        drawing.Circle(new(0, -5), 1);
                    }
                    else
                        drawing.Line(new(0, -4), new(0, -6), new("wire"));
                }

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, 6), new Vector2(0, 1));
            }
            private void DrawAreiPushSwitch(SvgDrawing drawing, bool lamp, bool window)
            {
                drawing.ExtendPin(Pins["a"]);
                drawing.Circle(new(), 4);
                drawing.Circle(new(), 2);

                if (lamp)
                {
                    double x = 2 / Math.Sqrt(2);
                    drawing.Path(b => b.MoveTo(-x, -x).LineTo(x, x).MoveTo(-x, x).LineTo(x, -x), new("lamp"));
                }

                if (window)
                {
                    drawing.Polyline(new Vector2[]
                    {
                        new(-6.5, 2.5), new(-4.5, 2.5), new(-4.5, 6), new(4.5, 6), new(4.5, 2.5), new(6.5, 2.5)
                    }, new("window"));
                }

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -5), new Vector2(0, -1));
            }
            private void DrawAreiSwitch(SvgDrawing drawing, bool toggling, bool doublePole, bool lamp)
            {
                double length = Math.Max(8, 5 + Math.Max(1, Poles) * 2);
                drawing.Circle(new(), 2);
                var n = Vector2.Normal(-Math.PI * 0.37);
                drawing.Line(n * 2, n * length);
                if (toggling)
                    drawing.Line(-n * 2, -n * length);
                if (doublePole)
                {
                    Vector2 np = new(-n.X, n.Y);
                    drawing.Line(np * 2, np * length);
                    if (toggling)
                        drawing.Line(-np * 2, -np * length);
                }

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -length), new Vector2(0, -1));

                // Small cross for illuminator lamps
                if (lamp)
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
                            if (toggling)
                                b.MoveTo(-r).LineTo(-end);
                            if (doublePole)
                            {
                                b.MoveTo(-r.X, r.Y).LineTo(-end.X, end.Y);
                                if (toggling)
                                    b.MoveTo(r.X, -r.Y).LineTo(end.X, -end.Y);
                            }
                            length -= 2.0;
                        }
                    });
                }
            }

            private void UpdateControlPin(bool closed, bool inverted)
            {
                if (inverted)
                {
                    if (closed)
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
                    if (closed)
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
            }
            private void UpdatePins(bool onewire, bool push)
            {
                if (onewire)
                {
                    if (push)
                    {
                        SetPinOffset(0, new(-4, 0));
                        SetPinOffset(3, new(4, 0));
                    }
                    else
                    {
                        SetPinOffset(0, new(-2, 0));
                        SetPinOffset(3, new(2, 0));
                    }
                }
                else
                {
                    SetPinOffset(0, new(-6, 0));
                    SetPinOffset(3, new(6, 0));
                }
            }
        }
    }
}