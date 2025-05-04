using SimpleCircuit.Components.Appearance;
using SimpleCircuit.Components.Builders;
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
    public abstract class ModelingDrawable : DiagramBlockInstance, IScaledDrawable, IBoxDrawable, IEllipseDrawable, IRoundedBox
    {
        public const string Square = "square";

        [Description("The size of the model block.")]
        public double Size { get; set; }

        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        [Description("The round-off corner radius.")]
        [Alias("r")]
        [Alias("radius")]
        public double CornerRadius { get; set; }

        Vector2 IBoxDrawable.TopLeft => -0.5 * new Vector2(Size, Size);
        Vector2 IBoxDrawable.BottomRight => 0.5 * new Vector2(Size, Size);
        Vector2 IEllipseDrawable.Center => new();
        double IEllipseDrawable.RadiusX => 0.5 * Size;
        double IEllipseDrawable.RadiusY => 0.5 * Size;

        /// <summary>
        /// Creates a new <see cref="ModelingDrawable"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        protected ModelingDrawable(string name, double size = 8)
            : base(name)
        {
            Size = size;
            Appearance.Background = "white";
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            if (Variants.Contains(Square))
                builder.Rectangle(-Size * 0.5, -Size * 0.5, Size, Size, Appearance, CornerRadius, CornerRadius);
            else
                builder.Circle(new(), Size * 0.5, Appearance);
        }

        /// <summary>
        /// Draws the labels for the drawable.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected void DrawLabels(IGraphicsBuilder builder)
        {
            if (Variants.Contains(Square))
                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this);
            else
                new OffsetAnchorPoints<IEllipseDrawable>(EllipseLabelAnchorPoints.Default, 1).Draw(builder, this);
        }

        /// <inheritdoc />
        protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
        {
            double s = Size * 0.5;
            if (Variants.Contains(Square))
            {
                // Calculate the vectors that will indicate
                double xp = Size * 0.5 - CornerRadius;
                Vector2 n1 = new(xp, Size * 0.5);
                Vector2 n2 = new(Size * 0.5, xp);
                double l = n1.Length;
                n1 /= l;
                n2 /= l;

                foreach (var pin in pins)
                {
                    double x = pin.Orientation.X;
                    double y = pin.Orientation.Y;
                    if (x.IsZero() && y.IsZero())
                    {
                        pin.Offset = new();
                        continue;
                    }

                    if (x > n1.X && y > n2.Y)
                    {
                        // Bottom-right corner
                        Vector2 c = new(xp, xp);
                        double k = c.Dot(pin.Orientation);
                        k += Math.Sqrt(k * k + CornerRadius * CornerRadius - c.X * c.X - c.Y * c.Y);
                        pin.Offset = k * pin.Orientation;
                        continue;
                    }
                    if (x > n1.X && y < -n2.Y)
                    {
                        // Top-right corner
                        Vector2 c = new(xp, -xp);
                        double k = c.Dot(pin.Orientation);
                        k += Math.Sqrt(k * k + CornerRadius * CornerRadius - c.X * c.X - c.Y * c.Y);
                        pin.Offset = k * pin.Orientation;
                        continue;
                    }
                    if (x < -n1.X && y < -n2.Y)
                    {
                        // Top-left corner
                        Vector2 c = new(-xp, -xp);
                        double k = c.Dot(pin.Orientation);
                        k += Math.Sqrt(k * k + CornerRadius * CornerRadius - c.X * c.X - c.Y * c.Y);
                        pin.Offset = k * pin.Orientation;
                        continue;
                    }
                    if (x < -n1.X && y > n2.Y)
                    {
                        // Bottom-left corner
                        Vector2 c = new(-xp, xp);
                        double k = c.Dot(pin.Orientation);
                        k += Math.Sqrt(k * k + CornerRadius * CornerRadius - c.X * c.X - c.Y * c.Y);
                        pin.Offset = k * pin.Orientation;
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
                    pin.Offset = pin.Orientation * s;
            }
        }
    }
}
