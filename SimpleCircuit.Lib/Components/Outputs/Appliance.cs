using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
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

            /// <inheritdoc />
            Vector2 IBoxDrawable.TopLeft => new(0, -8);

            /// <inheritdoc />
            Vector2 IBoxDrawable.Center => new(8, 0);

            /// <inheritdoc />
            Vector2 IBoxDrawable.BottomRight => new(16, 8);

            /// <inheritdoc />
            [Description("The round-off corner radius.")]
            [Alias("r")]
            [Alias("radius")]
            public double CornerRadius { get; set; }

            /// <inheritdoc />
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
                var style = builder.Style.Modify(Style);
                if (Variants.Contains(_heater))
                    DrawHeater(builder, Variants.Contains(_ventilator), Variants.Contains(_accu), style);
                else
                {
                    switch (Variants.Select(_ventilator, _boiler, _cooking, _microwave, _oven, _washer, _dryer, _dishwasher, _refrigerator, _fridge, _freezer))
                    {
                        case 0: DrawVentilator(builder, style); break;
                        case 1: DrawBoiler(builder, Variants.Contains(_accu), style); break;
                        case 2: DrawCooking(builder, style); break;
                        case 3: DrawMicroWave(builder, style); break;
                        case 4: DrawOven(builder, style); break;
                        case 5: DrawWasher(builder, style); break;
                        case 6: DrawDryer(builder, style); break;
                        case 7: DrawDishwasher(builder, style); break;
                        case 8:
                        case 9: DrawRefrigerator(builder, style); break;
                        case 10: DrawFreezer(builder, style); break;
                        default: DrawDefault(builder, style); break;
                    }
                }
                _anchors.Draw(builder, this, style);
            }
            private void DrawVentilator(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                DrawVentilator(builder, 8, 0, style);
            }
            private void DrawHeater(IGraphicsBuilder builder, bool ventilator, bool accumulator, IStyle style)
            {
                if (ventilator)
                {
                    DrawVentilator(builder, 17, 0, style, 3);
                    DrawHeater(builder, 7, 0, 10, 10, style);
                    DrawBox(builder, 11, 0, 22, 16, style);
                }
                else
                {
                    if (accumulator)
                    {
                        DrawBox(builder, 11, 0, 22, 16, style);
                        DrawHeater(builder, 11, 0, 16, 12, style);
                    }
                    else
                        DrawHeater(builder, 11, 0, 22, 16, style);
                }
            }
            private void DrawBoiler(IGraphicsBuilder builder, bool accumulator, IStyle style)
            {
                if (accumulator)
                {
                    builder.Circle(new(8, 0), 8, style);
                    DrawBoiler(builder, 8, 0, style, 6);
                }
                else
                    DrawBoiler(builder, 8, 0, style, 8);
            }
            private void DrawCooking(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                builder.Circle(new(4, -4), 2, style);
                builder.Circle(new(12, -4), 2, style);
                builder.Circle(new(12, 4), 2, style);
            }
            private void DrawMicroWave(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                DrawMicrowave(builder, 8, 0, style);
            }
            private void DrawOven(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                builder.Line(new(0, -5), new(16, -5), style);
                builder.Circle(new(8, 1.5), 2, style);
            }
            private void DrawWasher(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                builder.Circle(new(8, 0), 6, style);
                builder.Circle(new(8, 0), 1.5, style);
            }
            private void DrawDryer(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                DrawVentilator(builder, 8, -3, style);
                builder.Circle(new(8, 3), 1.5, style);
            }
            private void DrawDishwasher(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                DrawDishWasher(builder, 8, 0, style);
            }
            private void DrawRefrigerator(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 8, 0, 16, 16, style);
                DrawIce(builder, 8, 0, style);
            }
            private void DrawFreezer(IGraphicsBuilder builder, IStyle style)
            {
                DrawBox(builder, 14, 0, 28, 16, style);
                DrawIce(builder, 5, 0, style, 3.5);
                DrawIce(builder, 14, 0, style, 3.5);
                DrawIce(builder, 23, 0, style, 3.5);
            }
            private void DrawDefault(IGraphicsBuilder builder, IStyle style)
            {
                builder.ExtendPins(Pins, style);
                DrawBox(builder, 8, 0, 16, 16, style);
            }

            private void DrawBox(IGraphicsBuilder builder, double cx, double cy, double width, double height, IStyle style)
            {
                builder.Rectangle(cx - width * 0.5, cy - height * 0.5, width, height, style, CornerRadius, CornerRadius);
            }
            private void DrawVentilator(IGraphicsBuilder builder, double x, double y, IStyle style, double scale = 4)
            {
                builder.BeginTransform(new Transform(new(x, y), Matrix2.Identity));
                builder.Path(b => b.MoveTo(new(-scale, 0))
                    .CurveTo(new(-scale, -_k * scale), new(-scale * 0.5, -_k * scale * 0.75), new(0, 0))
                    .CurveTo(new(scale * 0.5, _k * scale * 0.75), new(scale, _k * scale), new(scale, 0))
                    .CurveTo(new(scale, -_k * scale), new(scale * 0.5, -_k * scale * 0.75), new(0, 0))
                    .CurveTo(new(-scale * 0.5, _k * scale * 0.75), new(-scale, _k * scale), new(-scale, 0))
                    .Close(), style);
                builder.EndTransform();
            }
            private void DrawHeater(IGraphicsBuilder builder, double cx, double cy, double width, double height, IStyle style)
            {
                DrawBox(builder, cx, cy, width, height, style);
                width /= 2.0;
                height /= 2.0;
                builder.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = cx + (i - 4) / 4.0 * width;
                        b.MoveTo(new(xi, cy - height)).LineTo(new(xi, cy + height));
                    }
                }, style);
            }
            private void DrawBoiler(IGraphicsBuilder builder, double cx, double cy, IStyle style, double r = 8)
            {
                builder.Circle(new(cx, cy), r, style);
                builder.Path(b =>
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        double xi = (i - 4) / 4.0 * r;
                        double yi = Math.Sqrt(r * r - xi * xi);
                        b.MoveTo(new(cx + xi, cy + yi)).LineTo(new(cx + xi, cy - yi));
                    }
                }, style);
            }
            private void DrawMicrowave(IGraphicsBuilder builder, double cx, double cy, IStyle style)
            {
                for (int i = -1; i <= 1; i++)
                {
                    double y = i * 3;
                    builder.BeginTransform(new Transform(new(cx, cy), Matrix2.Identity));
                    builder.Path(b => b.MoveTo(new(-4, y))
                    .CurveTo(new(-3, y - _k * 3), new(-1, y - _k * 3), new(0, y))
                    .CurveTo(new(1, y + _k * 3), new(3, y + _k * 3), new(4, y)), style);
                    builder.EndTransform();
                }
            }
            private void DrawDishWasher(IGraphicsBuilder builder, double cx, double cy, IStyle style, double s = 16)
            {
                s /= 2.0;
                double f = 3.0 / Math.Sqrt(2.0);
                builder.BeginTransform(new Transform(new(cx, cy), Matrix2.Identity));
                builder.Path(b => b
                    .MoveTo(new(-s, -s)).LineTo(new(-f, -f))
                    .MoveTo(new(s, -s)).LineTo(new(f, -f))
                    .MoveTo(new(s, s)).LineTo(new(f, f))
                    .MoveTo(new(-s, s)).LineTo(new(-f, f)), style);
                builder.Circle(new(), 3, style);
                builder.EndTransform();
            }
            private void DrawIce(IGraphicsBuilder builder, double cx, double cy, IStyle style, double scale = 6.0)
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
                }, style);
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