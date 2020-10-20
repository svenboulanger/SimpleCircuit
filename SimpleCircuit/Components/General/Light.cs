using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A light.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("LIGHT", "Light", Category = "Analog")]
    public class Light : TransformingComponent, ILabeled
    {
        private static double Sqrt2 = Math.Sqrt(2) * 2;

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Light(string name)
            : base(name)
        {
            Pins.Add(new[] { "a", "p" }, "The positive pin.", new Vector2(-6, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "b", "n" }, "The negative pin.", new Vector2(6, 0), new Vector2(1, 0));
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, 0), new Vector2(6, 0),
                new Vector2(-Sqrt2, -Sqrt2), new Vector2(Sqrt2, Sqrt2),
                new Vector2(Sqrt2, -Sqrt2), new Vector2(-Sqrt2, Sqrt2)
            });
            drawing.Circle(new Vector2(), 4);

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
    }
}
