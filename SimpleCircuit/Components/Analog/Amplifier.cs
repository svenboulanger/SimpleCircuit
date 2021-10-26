﻿namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An amplifier.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("A", "Amplifier", Category = "Analog")]
    public class Amplifier : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <inheritdoc/>
        public double Programmable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Amplifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Amplifier(string name)
            : base(name)
        {
            Pins.Add(new[] { "in" }, "The input.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "out" }, "The output.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polygon(new[]
            {
                new Vector2(-6, 6), new Vector2(6, 0), new Vector2(-6, -6)
            });

            if (!Programmable.IsZero())
            {
                // Programmable gain amplifier
                drawing.Polyline(new[] {
                    new Vector2(-6.5, 8), new Vector2(3, -7.5),
                    new Vector2(3, -7.5), new Vector2(3, -5.5),
                    new Vector2(3, -7.5), new Vector2(1.25, -6.5)
                });
            }

            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(-2.5, 0), new Vector2(), 3, 0.5);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Amplifier {Name}";
    }
}
