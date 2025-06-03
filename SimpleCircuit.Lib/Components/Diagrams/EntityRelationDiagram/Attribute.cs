using SimpleCircuit.Components.Pins;
using System.Collections.Generic;
using System;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    /// <summary>
    /// An ERD attribute.
    /// </summary>
    [Drawable("ATTR", "An entity-relationship diagram attribute.", "ERD", "ellipse")]
    public class Attribute : DrawableFactory
    {
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : DiagramBlockInstance(name), IEllipseDrawable
        {
            private double _width, _height;

            /// <inheritdoc />
            public override string Type => "attribute";

            [Description("The margin of the label inside the ADC when sizing based on content.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            [Description("The spacing between labels internally when sizing based on content.")]
            public Vector2 Spacing { get; set; }

            [Description("The width of the block. If 0, the size is calculated from the contents. The default is 0.")]
            [Alias("w")]
            public double Width { get; set; }

            [Description("The minimum width of the block. Only used when determining the width from contents.")]
            public double MinWidth { get; set; } = 0.0;

            [Description("The height of the block. If 0, the height is calculated from the contents. The default is 0.")]
            [Alias("h")]
            public double Height { get; set; }

            [Description("The minimum height of the block. Only used when determining the height from contents.")]
            public double MinHeight { get; set; } = 10.0;

            /// <inheritdoc />
            [Description("The label margin to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            /// <inheritdoc />
            Vector2 IEllipseDrawable.Center => new();

            /// <inheritdoc />
            double IEllipseDrawable.RadiusX => _width * 0.5;

            /// <inheritdoc />
            double IEllipseDrawable.RadiusY => _height * 0.5;

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                // When determining the size, let's update the size based on the label bounds
                switch (context.Mode)
                {
                    case PreparationMode.Sizes:
                        if (Width.IsZero() || Height.IsZero())
                        {
                            var bounds = EllipseLabelAnchorPoints.Default.CalculateSize(this, Spacing, Margin);
                            if (Width.IsZero())
                                _width = Math.Max(MinWidth, bounds.X);
                            if (Height.IsZero())
                                _height = Math.Max(MinHeight, bounds.Y);
                        }
                        else
                        {
                            _width = Width;
                            _height = Height;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);
                builder.Ellipse(new(), _width * 0.5, _height * 0.5, style);
                EllipseLabelAnchorPoints.Default.Draw(builder, this, style);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = _width * 0.5;
                double b = _height * 0.5;

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
