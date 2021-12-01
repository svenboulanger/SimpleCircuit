using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [SimpleKey("V", "A voltage source.", Category = "Sources")]
    public class VoltageSource : ScaledOrientedDrawable, ILabeled
    {
        /// <inheritdoc/>
        [Description("The label next to the source.")]
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public VoltageSource(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");

            if (options?.SmallSignal ?? false)
                AddVariant("ac");

            DrawingVariants = Variant.All(
                Variant.If("ac").DoElse(DrawAC, DrawDC),
                Variant.Do(DrawSource));
        }

        /// <inheritdoc/>
        private void DrawSource(SvgDrawing drawing)
        {
            // Wires
            drawing.Segments(new Vector2[]
            {
                new(-8, 0), new(-6, 0),
                new(6, 0), new(8, 0),
            }, new("wire"));

            // Circle
            drawing.Circle(new(0, 0), 6);

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
        }
        private void DrawDC(SvgDrawing drawing)
        {
            drawing.Line(new(-3, -1), new(-3, 1), new("minus"));
            drawing.Segments(new Vector2[]
            {
                    new(3, -1), new(3, 1),
                    new(2, 0), new(4, 0)
            }, new("plus"));
        }
        private void DrawAC(SvgDrawing drawing)
        {
            double hy = 1 / Math.Sqrt(2);
            double hx = hy * 2; // Slighly exaggerated curves
            drawing.OpenBezier(new Vector2[]
            {
                    new(0, -3),
                    new(hx, -3 + hy), new(hx, -hy), new(),
                    new(-hx, hy), new(-hx, 3 - hy), new(0, 3)
            });
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"Voltage source {Name}";
    }
}