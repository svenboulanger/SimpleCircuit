using System;
using System.Linq;

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
        public override bool TryCalculate(IBoxLabeled subject, string name, out LabelAnchorPoint value)
        {
            switch (name.ToLower())
            {
                case "0":
                case "c":
                    value = new(0.5 * (subject.TopLeft + subject.BottomRight), new());
                    return true; // Center

                case "1":
                case "nw":
                    Vector2 pt = 0.75 * subject.TopLeft + 0.25 * subject.BottomRight;
                    Vector2 n = new(-(subject.BottomRight.Y - subject.TopLeft.Y), -(subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    value = new(pt + subject.LabelMargin * n, n);
                    return true; // Top-left outside

                case "2":
                case "n":
                case "u":
                    double f = 0.0;
                    if (!subject.CornerRadius.IsZero())
                    {
                        n = subject.BottomRight - subject.TopLeft;
                        n /= n.Length;
                        f = subject.CornerRadius * (1 - Math.Sqrt(2 / (1 - n.X * n.X + n.Y * n.Y)));
                    }
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y - subject.LabelMargin + f), new(0, -1));
                    return true; // Top

                case "3":
                case "ne":
                    pt = new(0.25 * subject.TopLeft.X + 0.75 * subject.BottomRight.X, 0.75 * subject.TopLeft.Y + 0.25 * subject.BottomRight.Y);
                    n = new(subject.BottomRight.Y - subject.TopLeft.Y, -(subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    value = new(pt + subject.LabelMargin * n, n);
                    return true; // Top-right outside

                case "4":
                case "e":
                case "r":
                    value = new(new(subject.BottomRight.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0));
                    return true; // Right

                case "5":
                case "se":
                    pt = 0.25 * subject.TopLeft + 0.75 * subject.BottomRight;
                    n = new((subject.BottomRight.Y - subject.TopLeft.Y), (subject.BottomRight.X - subject.TopLeft.X));
                    n /= n.Length;
                    value = new(pt + subject.LabelMargin * n, n);
                    return true; // Bottom-right outside

                case "6":
                case "s":
                case "d":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y + subject.LabelMargin), new(0, 1));
                    return true; // Bottom

                case "7":
                case "sw":
                    pt = new(0.75 * subject.TopLeft.X + 0.25 * subject.BottomRight.X, 0.25 * subject.TopLeft.Y + 0.75 * subject.BottomRight.Y);
                    n = new(-(subject.BottomRight.Y - subject.TopLeft.Y), subject.BottomRight.X - subject.TopLeft.X);
                    n /= n.Length;
                    value = new(pt + subject.LabelMargin * n, n);
                    return true; // Bottom-left outside

                case "8":
                case "w":
                case "l":
                    value = new(new(subject.TopLeft.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));
                    return true; // Left

                default:
                    if (name.All(char.IsDigit))
                    {
                        int index = int.Parse(name);
                        index %= Count;
                        if (index < 0)
                            index += Count;
                        return TryCalculate(subject, index.ToString(), out value);
                    }
                    break;
            }

            value = default;
            return false;
        }
    }
}
