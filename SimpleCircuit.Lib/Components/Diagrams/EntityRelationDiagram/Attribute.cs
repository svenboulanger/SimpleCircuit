using SimpleCircuit.Components.Pins;
using System.Collections.Generic;
using System;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    [Drawable("ATTR", "An entity-relationship diagram attribute.", "ERD", "ellipse")]
    public class Attribute : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : DiagramBlockInstance(name), ILabeled, IEllipseLabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

            /// <inheritdoc />
            public override string Type => "attribute";

            /// <summary>
            /// Gets or sets the width of the attribute block.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width { get; set; } = 30;

            /// <summary>
            /// Gets or sets the height of the attribute block.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height { get; set; } = 20;

            [Description("The label margin to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IEllipseLabeled.Center => new();
            double IEllipseLabeled.RadiusX => Width * 0.5;
            double IEllipseLabeled.RadiusY => Height * 0.5;

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.Ellipse(new(), Width * 0.5, Height * 0.5);
                EllipseLabelAnchorPoints.Default.Draw(builder, this);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                foreach (var pin in pins)
                {
                    if (pin.Orientation.IsZero())
                        pin.Offset = new();
                    else
                    {
                        double nx = pin.Orientation.X;
                        double ny = pin.Orientation.Y;
                        double k = 1.0 / Math.Sqrt(nx * nx / b / b + ny * ny / a / a);

                        pin.Offset = new(
                            a * nx * k / b,
                            b * ny * k / a);
                    }
                }
            }
        }
    }
}
