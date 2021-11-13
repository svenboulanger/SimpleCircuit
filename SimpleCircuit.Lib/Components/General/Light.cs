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
        [Description("The label next to the light.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public Light(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new Vector2(-6, 0), new Vector2(-4, 0),
                new Vector2(4, 0), new Vector2(6, 0),
            }, new("wire"));

            // The light
            drawing.Segments(new[]
            {
                new Vector2(-_sqrt2, -_sqrt2), new Vector2(_sqrt2, _sqrt2),
                new Vector2(_sqrt2, -_sqrt2), new Vector2(-_sqrt2, _sqrt2)
            });
            drawing.Circle(new Vector2(), 4);

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
    }
}
