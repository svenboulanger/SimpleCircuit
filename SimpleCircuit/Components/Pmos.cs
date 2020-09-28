using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A PMOS transistor.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("Mp")]
    public class Pmos : IComponent
    {
        private readonly Unknown _x, _y, _nx, _ny, _s;

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        public Function X => _x;
        public Function Y => _y;
        public Function NormalX => _nx;
        public Function NormalY => _ny;
        public Function MirrorScale => _s;

        /// <summary>
        /// Gets or sets a value indicating whether the bulk contact should be rendered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bulk should be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBulk { get; set; } = false;

        public PinCollection Pins { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nmos"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Pmos(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new Unknown(name + ".x", UnknownTypes.X);
            _y = new Unknown(name + ".y", UnknownTypes.Y);
            _nx = new Unknown(name + ".nx", UnknownTypes.NormalX);
            _ny = new Unknown(name + ".ny", UnknownTypes.NormalY);
            _s = new Unknown(name + ".s", UnknownTypes.MirrorScale);
            Pins = new PinCollection(this);
            Pins.Add(new[] { "s", "source" }, new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "g", "gate" }, new Vector2(0, 11), new Vector2(0, 1));
            Pins.Add(new[] { "b", "bulk" }, new Vector2(0, 0), new Vector2(0, -1));
            Pins.Add(new[] { "d", "drain" }, new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(_nx.Value, _ny.Value);
            var tf = new Transform(_x.Value, _y.Value, normal, normal.Perpendicular * _s.Value);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(0, 11), new Vector2(0, 9),
                new Vector2(-6, 6), new Vector2(6, 6),
                new Vector2(-6, 4), new Vector2(6, 4)
            }));
            drawing.Circle(tf.Apply(new Vector2(0, 7.5)), 1.5);

            drawing.Poly(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-4, 0), new Vector2(-4, 4)
            }));
            drawing.Poly(tf.Apply(new[]
            {
                new Vector2(8, 0), new Vector2(4, 0), new Vector2(4, 4)
            }));

            if (ShowBulk)
                drawing.Line(tf.Apply(new Vector2(0, 4)), tf.Apply(new Vector2(0, 0)));

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, tf.Apply(new Vector2(2, -2)), tf.ApplyDirection(new Vector2(1, -1)));
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
