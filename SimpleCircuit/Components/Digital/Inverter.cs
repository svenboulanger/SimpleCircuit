﻿namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("INV", "Inverter", Category = "Digital")]
    public class Inverter : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Inverter(string name)
            : base(name)
        {
            Pins.Add(new[] { "in" }, "The input.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "out" }, "The output.", new Vector2(9, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
            });
            drawing.Circle(new Vector2(7.5, 0), 1.5);

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(-2.5, 0), new Vector2());
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Inverter {Name}";
    }
}
