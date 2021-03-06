﻿namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An operational amplifier.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("OA", "Opamp", Category = "Analog")]
    public class Opamp : TransformingComponent, ILabeled
    {
        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Opamp"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Opamp(string name)
            : base(name)
        {
            Pins.Add(new[] { "n" }, "The negative input.", new Vector2(-8, -4), new Vector2(-1, 0));
            Pins.Add(new[] { "p" }, "The positive input.", new Vector2(-8, 4), new Vector2(-1, 0));
            Pins.Add(new[] { "vn" }, "The power supply on the negative input side.", new Vector2(0, -6), new Vector2(0, -1));
            Pins.Add(new[] { "vp" }, "The power supply on the positive input side.", new Vector2(0, 6), new Vector2(0, 1));
            Pins.Add(new[] { "o", "out" }, "The output.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Polyline(new[] {
                new Vector2(-8, -8),
                new Vector2(8, 0),
                new Vector2(-8, 8),
                new Vector2(-8, -8)
            });
            drawing.Segments(new[]
            {
                // Plus
                new Vector2(-6, -4), new Vector2(-4, -4),

                // Minus
                new Vector2(-5, 5), new Vector2(-5, 3),
                new Vector2(-6, 4), new Vector2(-4, 4)
            });

            if (Pins.IsUsed("vn"))
                drawing.Line(new Vector2(0, -4), new Vector2(0, -6));
            if (Pins.IsUsed("vp"))
                drawing.Line(new Vector2(0, 4), new Vector2(0, 6));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(5, 5), new Vector2(1, 1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Opamp {Name}";
    }
}
