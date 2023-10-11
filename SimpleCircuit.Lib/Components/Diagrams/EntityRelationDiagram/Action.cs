using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    [Drawable("ACT", "An entity-relationship diagram action.", "ERD")]
    public class Action : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled, IBoxLabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "action";

            [Description("The width of the block.")]
            public double Width { get; set; } = 40;

            [Description("The height of the block.")]
            public double Height { get; set; } = 20;

            [Description("The margin of the label to the edge.")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxLabeled.TopLeft => new(-Width * 0.5, -Height * 0.5);
            Vector2 IBoxLabeled.BottomRight => new(Width * 0.5, Height * 0.5);
            double IBoxLabeled.CornerRadius => 0.0;

            /// <summary>
            /// Creates a new action.
            /// </summary>
            /// <param name="name">The name of the action.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.Path(builder =>
                {
                    double a = Width * 0.5, b = Height * 0.5;
                    builder.MoveTo(-a, 0)
                        .LineTo(0, -b)
                        .LineTo(a, 0)
                        .LineTo(0, b)
                        .Close();
                });
                DiamondLabelAnchorPoints.Default.Draw(drawing, this);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = 0.5 * Width;
                double b = 0.5 * Height;

                static Vector2 Interp(Vector2 a, Vector2 b, double ka)
                {
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }

                foreach (var pin in pins)
                {
                    double alpha = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                    if (alpha < -Math.PI * 0.5)
                        pin.Offset = Interp(new(-a, 0), new(0, -b), alpha + Math.PI);
                    else if (alpha < 0)
                        pin.Offset = Interp(new(0, -b), new(a, 0), alpha + Math.PI * 0.5);
                    else if (alpha < 0.5 * Math.PI)
                        pin.Offset = Interp(new(a, 0), new(0, b), alpha);
                    else
                        pin.Offset = Interp(new(0, b), new(-a, 0), alpha - Math.PI * 0.5);
                }
            }
        }
    }
}
