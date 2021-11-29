using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [SimpleKey("PLUG", "A wall plug.", Category = "Outputs")]
    public class Plug : ScaledOrientedDrawable, ILabeled
    {
        [Description("The label of the wall plug.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new wall plug.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        public Plug(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(), new(-1, 0)), "in", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(), new(1, 0)), "out", "b");

            DrawingVariants = Variant.All(
                Variant.Do(DrawPlug),
                Variant.If("earth").Do(DrawProtectiveConnection),
                Variant.If("sealed").Do(DrawSealed),
                Variant.If("child").Do(DrawChildProtection));
        }

        private void DrawPlug(SvgDrawing drawing)
        {
            drawing.Arc(new(4, 0), Math.PI / 2, -Math.PI / 2, 4, null, 1);
        }
        private void DrawProtectiveConnection(SvgDrawing drawing)
        {
            drawing.Line(new(0, 4), new(0, -4), new("earth"));
        }
        private void DrawChildProtection(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[]
            {
                new(-6, 4), new(-4, 4),
                new(4, 4), new(6, 4)
            }, new("child"));
        }
        private void DrawSealed(SvgDrawing drawing)
        {
            drawing.Text("h", new(0, -4), new(-1, -1));
        }
    }
}
