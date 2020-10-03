using SimpleCircuit.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleCircuit.Components
{
    [SimpleKey("D")]
    public class Diode : TransformingComponent, ILabeled
    {
        public string Label { get; set; }

        public Diode(string name)
            : base(name)
        {
            Pins.Add(new[] { "p", "a" }, "The anode.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "n", "b", "c" }, "The cathode.", new Vector2(6, 0), new Vector2(1, 0));
        }

        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            var tf = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);

            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, -4), new Vector2(4, 4),
                new Vector2(4, 0), new Vector2(6, 0)
            }));
            drawing.Polygon(tf.Apply(new[] {
                new Vector2(-4, -4),
                new Vector2(4, 0),
                new Vector2(-4, 4),
                new Vector2(-4, 4)
            }));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, -6)), tf.ApplyDirection(new Vector2(0, -1)));
        }

        public override void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X) + new Squared(Y);
            minimizer.AddConstraint(new Squared(Scale) - 1);
        }

        public override string ToString() => $"Diode {Name}";
    }
}
