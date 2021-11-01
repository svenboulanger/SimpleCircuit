using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A light symbol.
    /// </summary>
    [SimpleKey("LIGHT", "A light point.", Category = "Analog")]
    public class Light : ScaledOrientedDrawable, ILabeled
    {
        private static readonly double _sqrt2 = Math.Sqrt(2) * 2;

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Light(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, 0), new Vector2(6, 0),
                new Vector2(-_sqrt2, -_sqrt2), new Vector2(_sqrt2, _sqrt2),
                new Vector2(_sqrt2, -_sqrt2), new Vector2(-_sqrt2, _sqrt2)
            });
            drawing.Circle(new Vector2(), 4);

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
    }
}
