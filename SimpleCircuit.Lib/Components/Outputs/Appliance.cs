using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A fixed household appliance.
    /// </summary>
    [Drawable("APP", "A fixed household appliance.", "Outputs")]
    public class Appliance : DrawableFactory
    {
        private const string _ventilator = "ventilator";
        private const string _heater = "heater";
        private const string _boiler = "boiler";
        private const string _cooking = "cooking";
        private const string _microwave = "microwave";
        private const string _oven = "oven";
        private const string _washer = "washer";
        private const string _dryer = "dryer";
        private const string _dishwasher = "dishwasher";
        private const string _refrigerator = "refrigerator";
        private const string _fridge = "fridge";
        private const string _freezer = "freezer";
        private const string _accu = "accu";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private const double _k = 0.5522847498;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "appliance";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name of the appliance.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("p", "The connection.", this, new(), new(-1, 0)), "p", "a");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(_ventilator, _heater, _boiler, _cooking, _microwave, _oven, _washer, _dryer, _dishwasher, _refrigerator, _fridge, _freezer))
                {
                    case 0: DrawVentilator(drawing); break;
                    case 1: DrawHeater(drawing, Variants.Contains(_ventilator), Variants.Contains(_accu)); break;
                    case 2: DrawBoiler(drawing, Variants.Contains(_accu)); break;
                    case 3: DrawCooking(drawing); break;
                    case 4: DrawMicroWave(drawing); break;
                    case 5: DrawOven(drawing); break;
                    case 6: DrawWasher(drawing); break;
                    case 7: DrawDryer(drawing); break;
                    case 8: DrawDishwasher(drawing); break;
                    case 9:
                    case 10: DrawRefrigerator(drawing); break;
                    case 11: DrawFreezer(drawing); break;
                    default: DrawDefault(drawing); break;
                }
                DrawLabel(drawing);
            }
            private void DrawVentilator(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                DrawVentilator(drawing, 8, 0);
            }
            private void DrawHeater(SvgDrawing drawing, bool ventilator, bool accumulator)
            {
                if (ventilator)
                {
                    DrawVentilator(drawing, 17, 0, 3);
                    DrawHeater(drawing, 7, 0, 10, 10);
                    DrawBox(drawing, 11, 0, 22, 16);
                }
                else
                {
                    if (accumulator)
                    {
                        DrawBox(drawing, 11, 0, 22, 16);
                        DrawHeater(drawing, 11, 0, 16, 12);
                    }
                    else
                        DrawHeater(drawing, 11, 0, 22, 16);
                }
            }
            private void DrawBoiler(SvgDrawing drawing, bool accumulator)
            {
                if (accumulator)
                {
                    drawing.Circle(new(8, 0), 8);
                    DrawBoiler(drawing, 8, 0, 6);
                }
                else
                    DrawBoiler(drawing, 8, 0, 8);
            }
            private void DrawCooking(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                drawing.Circle(new(4, -4), 2, new("marker"));
                drawing.Circle(new(12, -4), 2, new("marker"));
                drawing.Circle(new(12, 4), 2, new("marker"));
            }
            private void DrawMicroWave(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                DrawMicrowave(drawing, 8, 0);
            }
            private void DrawOven(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                drawing.Line(new(0, -5), new(16, -5));
                drawing.Circle(new(8, 1.5), 2, new("marker"));
            }
            private void DrawWasher(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                drawing.Circle(new(8, 0), 6);
                drawing.Circle(new(8, 0), 1.5, new("marker"));
            }
            private void DrawDryer(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                DrawVentilator(drawing, 8, -3);
                drawing.Circle(new(8, 3), 1.5, new("marker"));
            }
            private void DrawDishwasher(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                DrawDishWasher(drawing, 8, 0);
            }
            private void DrawRefrigerator(SvgDrawing drawing)
            {
                DrawBox(drawing, 8, 0, 16, 16);
                DrawIce(drawing, 8, 0);
            }
            private void DrawFreezer(SvgDrawing drawing)
            {
                DrawBox(drawing, 14, 0, 28, 16);
                DrawIce(drawing, 5, 0, 3.5);
                DrawIce(drawing, 14, 0, 3.5);
                DrawIce(drawing, 23, 0, 3.5);
            }
            private void DrawDefault(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                DrawBox(drawing, 8, 0, 16, 16);
            }
            private void DrawLabel(SvgDrawing drawing)
            {
                drawing.Text(Labels[0], new(0, -10), new(1, -1));
            }

            private void DrawBox(SvgDrawing drawing, double cx, double cy, double width, double height)
            {
                width /= 2.0;
                height /= 2.0;
                drawing.Polygon(new Vector2[]
                {
                new(cx - width, cy - height), new(cx + width, cy - height),
                new(cx + width, cy + height), new(cx - width, cy + height)
                });
            }
            private void DrawVentilator(SvgDrawing drawing, double x, double y, double scale = 4)
            {
                drawing.ClosedBezier(new Vector2[]
                {
                new(-scale, 0),
                new(-scale, -_k * scale), new(-scale * 0.5, -_k * scale * 0.75), new(0, 0),
                new(scale * 0.5, _k * scale * 0.75), new(scale, _k * scale), new(scale, 0),
                new(scale, -_k * scale), new(scale * 0.5, -_k * scale * 0.75), new(0, 0),
                new(-scale * 0.5, _k * scale * 0.75), new(-scale, _k * scale), new(-scale, 0)
                }.Select(v => v + new Vector2(x, y)), new("ventilator"));
            }
            private void DrawHeater(SvgDrawing drawing, double cx, double cy, double width, double height)
            {
                DrawBox(drawing, cx, cy, width, height);
                width /= 2.0;
                height /= 2.0;
                drawing.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = cx + (i - 4) / 4.0 * width;
                        b.MoveTo(xi, cy - height).LineTo(xi, cy + height);
                    }
                }, new("heater"));
            }
            private void DrawBoiler(SvgDrawing drawing, double cx, double cy, double r = 8)
            {
                drawing.Circle(new(cx, cy), r);
                drawing.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = (i - 4) / 4.0 * r;
                        double yi = Math.Sqrt(r * r - xi * xi);
                        b.MoveTo(cx + xi, cy + yi).LineTo(cx + xi, cy - yi);
                    }
                }, new("boiler"));
            }
            private void DrawMicrowave(SvgDrawing drawing, double cx, double cy)
            {
                for (int i = -1; i <= 1; i++)
                {
                    double y = i * 3;
                    drawing.OpenBezier(new Vector2[]
                    {
                    new(-4, y),
                    new(-3, y - _k * 3), new(-1, y - _k * 3), new(0, y),
                    new(1, y + _k * 3), new(3, y + _k * 3), new(4, y)
                    }.Select(v => v + new Vector2(cx, cy)), new("microwave"));
                }
            }
            private void DrawDishWasher(SvgDrawing drawing, double cx, double cy, double s = 16)
            {
                s /= 2.0;
                double f = 3.0 / Math.Sqrt(2.0);
                drawing.BeginTransform(new Transform(new(cx, cy), Matrix2.Identity));
                drawing.Path(b => b
                    .MoveTo(-s, -s).LineTo(-f, -f)
                    .MoveTo(s, -s).LineTo(f, -f)
                    .MoveTo(s, s).LineTo(f, f)
                    .MoveTo(-s, s).LineTo(-f, f));
                drawing.Circle(new(), 3);
                drawing.EndTransform();
            }
            private void DrawIce(SvgDrawing drawing, double cx, double cy, double scale = 6.0)
            {
                double fx = Math.Cos(Math.PI / 3.0);
                double fy = Math.Sin(Math.PI / 3.0);
                var pts = IceFractal(new Vector2[]
                {
                    new(), new(1, 0),
                    new(), new(fx, fy),
                    new(), new(-fx, fy),
                    new(), new(-1, 0),
                    new(), new(-fx, -fy),
                    new(), new(fx, -fy)
                }, Math.PI / 6.0);

                drawing.Path(b =>
                {
                    b.WithTransform(new Transform(new(cx, cy), Matrix2.Scale(scale)));
                    int index = 0;
                    foreach (var g in pts.GroupBy(p => (index++) / 2))
                    {
                        var p = g.ToArray();
                        b.MoveTo(p[0]).LineTo(p[1]);
                    }
                }, new("ice"));
            }
            private IEnumerable<Vector2> IceFractal(IEnumerable<Vector2> points, double angle)
            {
                int index = 0;
                double corr = Math.Cos(angle);
                foreach (var g in points.GroupBy(p => (index++) / 2))
                {
                    var pts = g.ToArray();

                    // Calculate the fractal branching
                    var origin = 0.5 * (pts[0] + pts[1]);
                    var dir = pts[1] - pts[0];
                    var a = Math.Atan2(dir.Y, dir.X);
                    var p = origin + Vector2.Normal(a - angle) * dir.Length / 2.0 / corr;
                    yield return origin;
                    yield return p;

                    p = origin + Vector2.Normal(a + angle) * dir.Length / 2 / corr;
                    yield return origin;
                    yield return p;

                    yield return pts[0];
                    yield return pts[1];
                }
            }

            public override string ToString() => $"Appliance {Name}";
        }
    }
}