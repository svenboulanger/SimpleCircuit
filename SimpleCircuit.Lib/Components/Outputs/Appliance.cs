using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
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
    [Drawable("APP", "A fixed household appliance.", "Outputs", "ventilator heater boiler cooking microwave overn washer dryer dishwasher refrigerator fridge freezer accu arei")]
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

        private class Instance : ScaledOrientedDrawable, IBoxDrawable, IRoundedBox
        {
            private const double _k = 0.5522847498;
            private readonly static ILabelAnchorPoints<IBoxDrawable> _anchors = new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1);

            /// <inheritdoc />
            public override string Type => "appliance";

            Vector2 IBoxDrawable.TopLeft => new(0, -8);
            Vector2 IBoxDrawable.BottomRight => new(16, 8);

            [Description("The round-off corner radius.")]
            [Alias("r")]
            [Alias("radius")]
            public double CornerRadius { get; set; }

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                if (Variants.Contains(_heater))
                    DrawHeater(builder, Variants.Contains(_ventilator), Variants.Contains(_accu));
                else
                {
                    switch (Variants.Select(_ventilator, _boiler, _cooking, _microwave, _oven, _washer, _dryer, _dishwasher, _refrigerator, _fridge, _freezer))
                    {
                        case 0: DrawVentilator(builder); break;
                        case 1: DrawBoiler(builder, Variants.Contains(_accu)); break;
                        case 2: DrawCooking(builder); break;
                        case 3: DrawMicroWave(builder); break;
                        case 4: DrawOven(builder); break;
                        case 5: DrawWasher(builder); break;
                        case 6: DrawDryer(builder); break;
                        case 7: DrawDishwasher(builder); break;
                        case 8:
                        case 9: DrawRefrigerator(builder); break;
                        case 10: DrawFreezer(builder); break;
                        default: DrawDefault(builder); break;
                    }
                }
                _anchors.Draw(builder, this);
            }
            private void DrawVentilator(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                DrawVentilator(builder, 8, 0);
            }
            private void DrawHeater(IGraphicsBuilder builder, bool ventilator, bool accumulator)
            {
                if (ventilator)
                {
                    DrawVentilator(builder, 17, 0, 3);
                    DrawHeater(builder, 7, 0, 10, 10);
                    DrawBox(builder, 11, 0, 22, 16);
                }
                else
                {
                    if (accumulator)
                    {
                        DrawBox(builder, 11, 0, 22, 16);
                        DrawHeater(builder, 11, 0, 16, 12);
                    }
                    else
                        DrawHeater(builder, 11, 0, 22, 16);
                }
            }
            private void DrawBoiler(IGraphicsBuilder builder, bool accumulator)
            {
                if (accumulator)
                {
                    builder.Circle(new(8, 0), 8, Appearance);
                    DrawBoiler(builder, 8, 0, 6);
                }
                else
                    DrawBoiler(builder, 8, 0, 8);
            }
            private void DrawCooking(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                builder.Circle(new(4, -4), 2, Appearance);
                builder.Circle(new(12, -4), 2, Appearance);
                builder.Circle(new(12, 4), 2, Appearance);
            }
            private void DrawMicroWave(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                DrawMicrowave(builder, 8, 0);
            }
            private void DrawOven(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                builder.Line(new(0, -5), new(16, -5), Appearance);
                builder.Circle(new(8, 1.5), 2, Appearance);
            }
            private void DrawWasher(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                builder.Circle(new(8, 0), 6, Appearance);
                builder.Circle(new(8, 0), 1.5, Appearance);
            }
            private void DrawDryer(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                DrawVentilator(builder, 8, -3);
                builder.Circle(new(8, 3), 1.5, Appearance);
            }
            private void DrawDishwasher(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                DrawDishWasher(builder, 8, 0);
            }
            private void DrawRefrigerator(IGraphicsBuilder builder)
            {
                DrawBox(builder, 8, 0, 16, 16);
                DrawIce(builder, 8, 0);
            }
            private void DrawFreezer(IGraphicsBuilder builder)
            {
                DrawBox(builder, 14, 0, 28, 16);
                DrawIce(builder, 5, 0, 3.5);
                DrawIce(builder, 14, 0, 3.5);
                DrawIce(builder, 23, 0, 3.5);
            }
            private void DrawDefault(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance);
                DrawBox(builder, 8, 0, 16, 16);
            }

            private void DrawBox(IGraphicsBuilder builder, double cx, double cy, double width, double height)
            {
                builder.Rectangle(cx - width * 0.5, cy - height * 0.5, width, height, CornerRadius, CornerRadius);
            }
            private void DrawVentilator(IGraphicsBuilder builder, double x, double y, double scale = 4)
            {
                builder.BeginTransform(new Transform(new(x, y), Matrix2.Identity));
                builder.Path(b => b.MoveTo(new(-scale, 0))
                    .CurveTo(new(-scale, -_k * scale), new(-scale * 0.5, -_k * scale * 0.75), new(0, 0))
                    .CurveTo(new(scale * 0.5, _k * scale * 0.75), new(scale, _k * scale), new(scale, 0))
                    .CurveTo(new(scale, -_k * scale), new(scale * 0.5, -_k * scale * 0.75), new(0, 0))
                    .CurveTo(new(-scale * 0.5, _k * scale * 0.75), new(-scale, _k * scale), new(-scale, 0))
                    .Close(), new("ventilator"));
                builder.EndTransform();
            }
            private void DrawHeater(IGraphicsBuilder builder, double cx, double cy, double width, double height)
            {
                DrawBox(builder, cx, cy, width, height);
                width /= 2.0;
                height /= 2.0;
                builder.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = cx + (i - 4) / 4.0 * width;
                        b.MoveTo(new(xi, cy - height)).LineTo(new(xi, cy + height));
                    }
                }, new("heater"));
            }
            private void DrawBoiler(IGraphicsBuilder builder, double cx, double cy, double r = 8)
            {
                builder.Circle(new(cx, cy), r, Appearance);
                builder.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = (i - 4) / 4.0 * r;
                        double yi = Math.Sqrt(r * r - xi * xi);
                        b.MoveTo(new(cx + xi, cy + yi)).LineTo(new(cx + xi, cy - yi));
                    }
                }, new("boiler"));
            }
            private void DrawMicrowave(IGraphicsBuilder builder, double cx, double cy)
            {
                for (int i = -1; i <= 1; i++)
                {
                    double y = i * 3;
                    builder.BeginTransform(new Transform(new(cx, cy), Matrix2.Identity));
                    builder.Path(b => b.MoveTo(new(-4, y))
                    .CurveTo(new(-3, y - _k * 3), new(-1, y - _k * 3), new(0, y))
                    .CurveTo(new(1, y + _k * 3), new(3, y + _k * 3), new(4, y)), new("microwave"));
                    builder.EndTransform();
                }
            }
            private void DrawDishWasher(IGraphicsBuilder builder, double cx, double cy, double s = 16)
            {
                s /= 2.0;
                double f = 3.0 / Math.Sqrt(2.0);
                builder.BeginTransform(new Transform(new(cx, cy), Matrix2.Identity));
                builder.Path(b => b
                    .MoveTo(new(-s, -s)).LineTo(new(-f, -f))
                    .MoveTo(new(s, -s)).LineTo(new(f, -f))
                    .MoveTo(new(s, s)).LineTo(new(f, f))
                    .MoveTo(new(-s, s)).LineTo(new(-f, f)));
                builder.Circle(new(), 3, Appearance);
                builder.EndTransform();
            }
            private void DrawIce(IGraphicsBuilder builder, double cx, double cy, double scale = 6.0)
            {
                double fx = Math.Cos(Math.PI / 3.0);
                double fy = Math.Sin(Math.PI / 3.0);
                var pts = IceFractal([
                    new(), new(1, 0),
                    new(), new(fx, fy),
                    new(), new(-fx, fy),
                    new(), new(-1, 0),
                    new(), new(-fx, -fy),
                    new(), new(fx, -fy)
                ], Math.PI / 6.0);

                builder.BeginTransform(new Transform(new(cx, cy), Matrix2.Scale(scale)));
                builder.Path(b =>
                {
                    int index = 0;
                    foreach (var g in pts.GroupBy(p => (index++) / 2))
                    {
                        var p = g.ToArray();
                        b.MoveTo(p[0]).LineTo(p[1]);
                    }
                }, new("ice"));
                builder.EndTransform();
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