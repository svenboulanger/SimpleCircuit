using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A fixed household appliance.
    /// </summary>
    [SimpleKey("APP", "A fixed household appliance.", Category = "Outputs")]
    public class Appliance : ScaledOrientedDrawable, ILabeled
    {
        private const double _k = 0.5522847498;

        /// <summary>
        /// The possible types of appliances.
        /// </summary>
        [Flags]
        public enum Variants
        {
            [Description("A fixed appliance.")]
            None,

            [Description("A heater.")]
            Heater = 1,

            [Description("A boiler.")]
            Boiler = 2,

            [Description("A cooking plates.")]
            Cooking = 3,

            [Description("A microwave.")]
            Microwave = 4,

            [Description("An oven.")]
            Oven = 5,

            [Description("A washer.")]
            Washer = 6,

            [Description("A dryer.")]
            Dryer = 7,

            [Description("A dishwasher.")]
            DishWasher = 8,

            [Description("A refrigerator.")]
            Refrigerator = 9,

            [Description("A freezer.")]
            Freezer = 10,

            [Description("With a ventilator")]
            Ventilator = 0x10,

            [Description("With an accumulator.")]
            Accumulator = 0x20
        }

        [Description("Adds a label next to the appliance.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new appliance.
        /// </summary>
        /// <param name="name">The name of the appliance.</param>
        /// <param name="options">The options.</param>
        public Appliance(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("p", "The connection.", this, new(), new(-1, 0)), "p", "a");

            DrawingVariants = Variant.All(
                Variant.FirstOf(
                    Variant.If("ventilator").Do(DrawVentilator),
                    Variant.If("heater").Do(Variant.Map("ventilator", "accu", DrawHeater)),
                    Variant.If("boiler").Do(Variant.Map("accu", DrawBoiler)),
                    Variant.If("cooking").Do(DrawCooking),
                    Variant.If("microwave").Do(DrawMicroWave),
                    Variant.If("oven").Do(DrawOven),
                    Variant.If("washer").Do(DrawWasher),
                    Variant.If("dryer").Do(DrawDryer),
                    Variant.If("dishwasher").Do(DrawDishwasher),
                    Variant.If("refrigerator").Do(DrawRefrigerator),
                    Variant.If("fridge").Do(DrawRefrigerator),
                    Variant.If("freezer").Do(DrawFreezer),
                    Variant.Do(DrawDefault)
                    ),
                Variant.Do(DrawLabel));
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
            drawing.Circle(new(4, -4), 2, new("dot"));
            drawing.Circle(new(12, -4), 2, new("dot"));
            drawing.Circle(new(12, 4), 2, new("dot"));
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
            drawing.Circle(new(8, 1.5), 2, new("dot"));
        }
        private void DrawWasher(SvgDrawing drawing)
        {
            DrawBox(drawing, 8, 0, 16, 16);
            drawing.Circle(new(8, 0), 6);
            drawing.Circle(new(8, 0), 1.5, new("dot"));
        }
        private void DrawDryer(SvgDrawing drawing)
        {
            DrawBox(drawing, 8, 0, 16, 16);
            DrawVentilator(drawing, 8, -3);
            drawing.Circle(new(8, 3), 1.5, new("dot"));
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
            DrawBox(drawing, 8, 0, 16, 16);
        }
        private void DrawLabel(SvgDrawing drawing)
        {
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(0, -10), new(1, -1));
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
            List<Vector2> points = new(14);
            for (int i = 1; i <= 7; i++)
            {
                double xi = cx + (i - 4) / 4.0 * width;
                points.Add(new(xi, cy - height));
                points.Add(new(xi, cy + height));
            }
            drawing.Segments(points, new("heater"));
        }
        private void DrawBoiler(SvgDrawing drawing, double cx, double cy, double r = 8)
        {
            drawing.Circle(new(cx, cy), r);
            List<Vector2> points = new(14);
            for (int i = 1; i <= 7; i++)
            {
                double xi = (i - 4) / 4.0 * r;
                double yi = Math.Sqrt(r * r - xi * xi);
                points.Add(new(cx + xi, cy + yi));
                points.Add(new(cx + xi, cy - yi));
            }
            drawing.Segments(points, new("boiler"));
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
            drawing.Segments(new Vector2[]
            {
                new(-s, -s), new(-f, -f),
                new(s, -s), new(f, -f),
                new(s, s), new(f, f),
                new(-s, s), new(-f, f)
            }.Select(v => v + new Vector2(cx, cy)));
            drawing.Circle(new(cx, cy), 3);
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

            drawing.Segments(pts.Select(v => v * scale + new Vector2(cx, cy)));
        }
        private IEnumerable<Vector2> IceFractal(IEnumerable<Vector2> points, double angle)
        {
            int index = 0;
            double corr = Math.Cos(angle);
            foreach (var g in points.GroupBy(p => (index++)/2))
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
