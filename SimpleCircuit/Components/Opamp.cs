using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    [SimpleKey("OA")]
    public class Opamp : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny, _s;

        public string Name { get; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => _nx;
        public Function NormalY => _ny;
        public Function MirrorScale => _s;
        public PinCollection Pins { get; }

        public Opamp(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.NormalX);
            _ny = new Unknown(name + ".ny", UnknownTypes.NormalY);
            _s = new Unknown(name + ".s", UnknownTypes.MirrorScale);
            Pins = new PinCollection(this);
            Pins.Add(new[] { "-", "n" }, new Vector2(-8, -4), new Vector2(-1, 0));
            Pins.Add(new[] { "+", "p" }, new Vector2(-8, 4), new Vector2(-1, 0));
            Pins.Add(new[] { "o", "out" }, new Vector2(8, 0), new Vector2(1, 0));
        }

        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular * _s.Value);
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

        /// <summary>
        /// Applies some functions to the minimizer if necessary.
        /// </summary>
        /// <param name="minimizer">The minimizer.</param>
        public void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(_x) + new Squared(_y) + new Squared(_nx) + new Squared(_ny - 1);
            minimizer.AddConstraint(new Squared(_s) - 1);
        }
    }
}
