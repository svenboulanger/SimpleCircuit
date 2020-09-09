using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// An inductor.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("L")]
    public class Inductor : IComponent
    {
        private readonly Contributor _x, _y, _sx, _sy, _a;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<IPin> Pins { get; }

        /// <inheritdoc/>
        public IEnumerable<Contributor> Contributors => new Contributor[] { _x, _y, _sx, _sy, _a };

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Inductor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            _sx = new ConstantContributor(UnknownTypes.ScaleX, 1.0);
            _sy = new DirectContributor(name + ".SY", UnknownTypes.ScaleY);
            _a = new DirectContributor(name + ".A", UnknownTypes.Angle);
            Pins = new[]
            {
                new Pin(this, _x, _y, _sx, _sy, _a, new Vector2(-8, 0), Math.PI, new[] { "p", "+", "pos", "a" }),
                new Pin(this, _x, _y, _sx, _sy, _a, new Vector2(8, 0), 0, new[] { "n", "-", "neg", "b" })
            };
        }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            var tf = new Transform(_x.Value, _y.Value, _sx.Value, _sy.Value, _a.Value);

            drawing.Segments(tf.Apply(new[]
            {
                new Vector2(-8, 0), new Vector2(-6, 0),
                new Vector2(6, 0), new Vector2(8, 0)
            }));
            drawing.SmoothBezier(tf.Apply(new[]
            {
                new Vector2(-6, 0),
                new Vector2(-6, -4), new Vector2(-2, -4), new Vector2(-2, 0),
                new Vector2(-4, 4), new Vector2(-4, 0),
                new Vector2(1, -4), new Vector2(1, 0),
                new Vector2(-1, 4), new Vector2(-1, 0),
                new Vector2(4, -4), new Vector2(4, 0),
                new Vector2(2, 4), new Vector2(2, 0),
                new Vector2(6, -4), new Vector2(6, 0)
            }));

            drawing.Text(Label, tf.Apply(new Vector2(0, -6)), tf.ApplyDirection(new Vector2(0, -1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Inductor {Name}";
    }
}
