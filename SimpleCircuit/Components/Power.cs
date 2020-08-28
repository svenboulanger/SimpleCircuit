using SimpleCircuit.Contributions;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A power supply.
    /// </summary>
    /// <seealso cref="IComponent" />
    [SimpleKey("POW")]
    public class Power : IComponent
    {
        private readonly IContributor _x, _y, _a;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<IPin> Pins { get; }

        /// <inheritdoc/>
        public IEnumerable<IContributor> Contributors => new IContributor[] { _x, _y, _a };

        /// <summary>
        /// Initializes a new instance of the <see cref="Ground"/> class.
        /// </summary>
        public Power(string name)
        {
            Name = name;
            _x = new DirectContributor(name + ".X", UnknownTypes.X);
            _y = new DirectContributor(name + ".Y", UnknownTypes.Y);
            _a = new DirectContributor(name + ".A", UnknownTypes.Angle);
            Pins = new[]
            {
                new Pin(this, _x, _y, 1.0.SX(), 1.0.SY(), _a, new Vector2(), -Math.PI / 2, new[] { ".", "a" })
            };
        }

        /// <inheritdoc/>
        public void Render(SvgDrawing drawing)
        {
            var tf = new Transform(_x.Value, _y.Value, _a.Value);
            drawing.Line(tf.Apply(new Vector2(0, 0)), tf.Apply(new Vector2(0, 3)));
            drawing.Line(tf.Apply(new Vector2(-5, 3)), tf.Apply(new Vector2(5, 3)), "plane");
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, tf.Apply(new Vector2(0, 6)), tf.ApplyDirection(new Vector2(0, 1)));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Power {Name}";
    }
}
