using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A light symbol.
    /// </summary>
    [SimpleKey("LIGHT", "A light point.", Category = "Outputs")]
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
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");

            if (options?.ElectricalInstallation ?? false)
                AddVariant("eic");
            DrawingVariants = Variant.All(
                Variant.Do(DrawLamp),
                Variant.IfNot("eic").Do(DrawCasing));
            PinUpdate = Variant.Map("eic", UpdatePins);
        }

        /// <inheritdoc/>
        private void DrawLamp(SvgDrawing drawing)
        {
            // The light
            drawing.Segments(new[]
            {
                new Vector2(-_sqrt2, -_sqrt2), new Vector2(_sqrt2, _sqrt2),
                new Vector2(_sqrt2, -_sqrt2), new Vector2(-_sqrt2, _sqrt2)
            });

            // Label
            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
        }
        private void DrawCasing(SvgDrawing drawing)
        {
            drawing.Circle(new Vector2(), 4);
        }
        private void UpdatePins(bool eic)
        {
            if (eic)
            {
                ((FixedOrientedPin)Pins[0]).Offset = new();
                ((FixedOrientedPin)Pins[1]).Offset = new();
            }
            else
            {
                ((FixedOrientedPin)Pins[0]).Offset = new(-4, 0);
                ((FixedOrientedPin)Pins[1]).Offset = new(4, 0);
            }
        }
    }
}
