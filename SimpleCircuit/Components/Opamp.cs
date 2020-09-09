using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components
{
    [SimpleKey("OA")]
    public class Opamp : IComponent
    {
        private readonly Contributor _x, _y, _sx, _sy, _a;

        public string Name { get; }

        public IReadOnlyList<IPin> Pins { get; }

        public IEnumerable<Contributor> Contributors => new Contributor[] { _x, _y, _sx, _sy, _a };

        public Opamp(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            _sx = new ConstantContributor(UnknownTypes.ScaleX, 1.0);
            _sy = new DirectContributor(name + ".SY", UnknownTypes.ScaleY);
            _a = new DirectContributor(name + ".A", UnknownTypes.Angle);
            Pins = new[]
            {
                new Pin(this, _x, _y, _sx, _sy, _a, new Vector2(-8, -4), Math.PI, new[] { "-", "n" }),
                new Pin(this, _x, _y, _sx, _sy, _a, new Vector2(-8, 4), Math.PI, new[] { "+", "p" }),
                new Pin(this, _x, _y, _sx, _sy, _a, new Vector2(8, 0), 0, new[] { "o", "out" }),
            };
        }

        public void Render(SvgDrawing drawing)
        {
            var tf = new Transform(_x.Value, _y.Value, _sx.Value, _sy.Value, _a.Value);
            drawing.Poly(tf.Apply(new[] {
                new Vector2(-8, -8),
                new Vector2(8, 0),
                new Vector2(-8, 8),
                new Vector2(-8, -8)
            }));
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-6, -4), new Vector2(-4, -4),
                new Vector2(-5, 5), new Vector2(-5, 3),
                new Vector2(-6, 4), new Vector2(-4, 4)
            }));
        }
    }
}
