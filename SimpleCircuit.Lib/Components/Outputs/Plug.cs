using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [Drawable("WP", "A wall plug.", "Outputs")]
    public class Plug : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label of the wall plug.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
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
                new(4, -6), new(4, -4),
                new(4, 4), new(4, 6)
                }, new("child"));
            }
            private void DrawSealed(SvgDrawing drawing)
            {
                drawing.Text("h", new(0, -4), new(-1, -1));
            }
        }
    }
}