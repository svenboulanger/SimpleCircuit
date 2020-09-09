using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A ground terminal.
    /// </summary>
    /// <seealso cref="Component" />
    [SimpleKey("GND")]
    public class Ground : IComponent
    {
        private readonly Contributor _x, _y, _a;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPin> Pins { get; }

        /// <inheritdoc/>
        public IEnumerable<Contributor> Contributors => new Contributor[] { _x, _y, _a };

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        public Ground(string name)
        {
            Name = name;
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            _a = new DirectContributor(name + ".A", UnknownTypes.Angle);
            Pins = new[]
            {
                new Pin(this, _x, _y, new ConstantContributor(UnknownTypes.ScaleX, 1.0), new ConstantContributor(UnknownTypes.ScaleY, 1.0), _a, new Vector2(), Math.PI / 2, new[] { ".", "a" })
            };
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var tf = new Transform(_x.Value, _y.Value, _a.Value);
            drawing.Segments(tf.Apply(new Vector2[]
            {
                new Vector2(0, 0), new Vector2(0, -3),
                new Vector2(-5, -3), new Vector2(5, -3),
                new Vector2(-3, -5), new Vector2(3, -5),
                new Vector2(-1, -7), new Vector2(1, -7)
            }));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Ground {Name}";
    }
}
