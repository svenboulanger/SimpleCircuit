using System;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Label anchor points for a diamond shape.
    /// </summary>
    public class DiamondLabelAnchorPoints : LabelAnchorPoints<IBoxLabeled>
    {
        /// <summary>
        /// Gets the diamond label anchor points.
        /// </summary>
        public static DiamondLabelAnchorPoints Default { get; } = new DiamondLabelAnchorPoints();

        /// <inheritdoc />
        public override int Count => 9;

        /// <summary>
        /// Creates a new <see cref="DiamondLabelAnchorPoints"/>.
        /// </summary>
        protected DiamondLabelAnchorPoints()
        {
        }

        /// <inheritdoc />
        public override LabelAnchorPoint Calculate(IBoxLabeled subject, int index)
        {
            index %= Count;
            if (index < 0)
                index += Count;

            switch (index)
            {
                case 0:
                    return new(0.5 * (subject.TopLeft + subject.BottomRight), new()); // Center

                case 1:
                    Vector2 pt = 0.75 * subject.TopLeft + 0.25 * subject.BottomRight;
                    Vector2 n = new(-(subject.BottomRight.Y - subject.TopLeft.Y), -(subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    return new(pt + subject.LabelMargin * n, n); // Top-left outside
                case 2:
                    double f = 0.0;
                    if (!subject.CornerRadius.IsZero())
                    {
                        n = subject.BottomRight - subject.TopLeft;
                        n /= n.Length;
                        f = subject.CornerRadius * (1 - Math.Sqrt(2 / (1 - n.X * n.X + n.Y * n.Y)));
                    }
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y - subject.LabelMargin + f), new(0, -1)); // Top
                case 3:
                    pt = new(0.25 * subject.TopLeft.X + 0.75 * subject.BottomRight.X, 0.75 * subject.TopLeft.Y + 0.25 * subject.BottomRight.Y);
                    n = new(subject.BottomRight.Y - subject.TopLeft.Y, -(subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    return new(pt + subject.LabelMargin * n, n); // Top-right outside
                case 4:
                    return new(new(subject.BottomRight.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0)); // Right
                case 5:
                    pt = 0.25 * subject.TopLeft + 0.75 * subject.BottomRight;
                    n = new((subject.BottomRight.Y - subject.TopLeft.Y), (subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    return new(pt + subject.LabelMargin * n, n); // Bottom-right outside
                case 6:
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y + subject.LabelMargin), new(0, 1)); // Bottom
                case 7:
                    pt = new(0.75 * subject.TopLeft.X + 0.25 * subject.BottomRight.X, 0.25 * subject.TopLeft.Y + 0.75 * subject.BottomRight.Y);
                    n = new(-(subject.BottomRight.Y - subject.TopLeft.Y), subject.BottomRight.X - subject.TopLeft.X);
                    n /= n.Length;
                    return new(pt + subject.LabelMargin * n, n); // Bottom-left outside
                case 8:
                    return new(new(subject.TopLeft.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0)); // Left
            }
            return new();
        }
    }
}
