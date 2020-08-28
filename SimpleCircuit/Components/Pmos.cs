using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A PMOS transistor.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("Mp")]
    public class Pmos : IComponent
    {
        private readonly IContributor _x, _y, _sx, _sy, _a;

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bulk contact should be rendered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bulk should be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBulk { get; set; } = false;

        /// <inheritdoc />
        public IReadOnlyList<IPin> Pins { get; }

        /// <inheritdoc />
        public IEnumerable<IContributor> Contributors => new IContributor[] { _x, _y, _sx, _sy, _a };

        /// <summary>
        /// Initializes a new instance of the <see cref="Nmos"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Pmos(string name)
        {
            Name = name;
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            _sx = new DirectContributor(name + ".SX", UnknownTypes.ScaleX);
            _sy = new ConstantContributor(UnknownTypes.ScaleY, 1.0);
            _a = new DirectContributor(name + ".A", UnknownTypes.Angle);
            var sy = 1.0.SY();
            Pins = new[]
            {
                new Pin(this, _x, _y, _sx, sy, _a, new Vector2(0, -8), -Math.PI / 2, new[] { "s", "source" }),
                new Pin(this, _x, _y, _sx, sy, _a, new Vector2(-11, 0), Math.PI, new[] { "g", "gate" }),
                new Pin(this, _x, _y, _sx, sy, _a, new Vector2(0, 0), 0, new[] { "b", "bulk" }),
                new Pin(this, _x, _y, _sx, sy, _a, new Vector2(0, 8), Math.PI / 2, new[] { "d", "drain" }),
            };
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var tf = new Transform(_x.Value, _y.Value, _sx.Value, _sy.Value, _a.Value);
            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-11, 0), new Vector2(-9, 0),
                new Vector2(-6, 6), new Vector2(-6, -6),
                new Vector2(-4, 6), new Vector2(-4, -6)
            }));
            drawing.Circle(tf.Apply(new Vector2(-7.5, 0)), 1.5);

            drawing.Poly(tf.Apply(new[]
            {
                new Vector2(-4, 4), new Vector2(0, 4), new Vector2(0, 8)
            }));
            drawing.Poly(tf.Apply(new[]
            {
                new Vector2(-4, -4), new Vector2(0, -4), new Vector2(0, -8)
            }));

            if (ShowBulk)
                drawing.Line(tf.Apply(new Vector2(-4, 0)), tf.Apply(new Vector2(0, 0)));

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, tf.Apply(new Vector2(4, 4)), tf.ApplyDirection(new Vector2(1, 1)));
        }
    }
}
