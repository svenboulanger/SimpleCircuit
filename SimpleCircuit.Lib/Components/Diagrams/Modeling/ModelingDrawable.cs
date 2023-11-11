using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    /// <summary>
    /// A generic drawable used for modeling block diagrams.
    /// These blocks don't have an orientation, but they can be square or circular and have 8 pins in all major directions.
    /// </summary>
    public abstract class ModelingDrawable : DiagramBlockInstance, IScaledDrawable, IBoxLabeled, IEllipseLabeled
    {
        public const string Square = "square";

        /// <summary>
        /// Gets the width of the drawable.
        /// </summary>
        protected virtual double Size => 8;

        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <inheritdoc />
        public Labels Labels { get; } = new Labels();

        Vector2 IBoxLabeled.TopLeft => -0.5 * new Vector2(Size, Size);
        Vector2 IBoxLabeled.BottomRight => 0.5 * new Vector2(Size, Size);
        double IBoxLabeled.CornerRadius => 0.0;
        Vector2 IEllipseLabeled.Center => new();
        double IEllipseLabeled.RadiusX => 0.5 * Size;
        double IEllipseLabeled.RadiusY => 0.5 * Size;

        /// <summary>
        /// Creates a new <see cref="ModelingDrawable"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        protected ModelingDrawable(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            if (Variants.Contains(Square))
                drawing.Rectangle(-Size * 0.5, -Size * 0.5, Size, Size);
            else
                drawing.Circle(new(), Size * 0.5);
        }

        /// <summary>
        /// Draws the labels for the drawable.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected void DrawLabels(SvgDrawing drawing)
        {
            if (Variants.Contains(Square))
                new OffsetAnchorPoints<IBoxLabeled>(BoxLabelAnchorPoints.Default, 1).Draw(drawing, this);
            else
                new OffsetAnchorPoints<IEllipseLabeled>(EllipseLabelAnchorPoints.Default, 1).Draw(drawing, this);
        }

        /// <inheritdoc />
        protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
        {
            double s = Size * 0.5;
            if (Variants.Contains(Square))
            {
                foreach (var pin in pins)
                {
                    double x = pin.Orientation.X;
                    double y = pin.Orientation.Y;
                    if (x.IsZero() && y.IsZero())
                    {
                        pin.Offset = new();
                        continue;
                    }

                    if (x - y < 0)
                    {
                        if (x + y < 0)
                            pin.Offset = new(-s, y / Math.Abs(x) * s);
                        else
                            pin.Offset = new(x / Math.Abs(y) * s, s);
                    }
                    else
                    {
                        if (x + y < 0)
                            pin.Offset = new(x / Math.Abs(y) * s, -s);
                        else
                            pin.Offset = new(s, y / Math.Abs(x) * s);
                    }
                }
            }
            else
            {
                // Assume a circle
                foreach (var pin in pins)
                {
                    pin.Offset = pin.Orientation * s;
                }
            }
        }
    }
}
