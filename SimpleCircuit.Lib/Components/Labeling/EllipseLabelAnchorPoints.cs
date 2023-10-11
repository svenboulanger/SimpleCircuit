using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of label anchor points that can be used for a circle.
    /// </summary>
    public class EllipseLabelAnchorPoints : LabelAnchorPoints<IEllipseLabeled>
    {
        /// <summary>
        /// Gets the ellipse label anchor points.
        /// </summary>
        public static EllipseLabelAnchorPoints Default { get; } = new EllipseLabelAnchorPoints();

        /// <inheritdoc />
        public override int Count => 9;

        /// <summary>
        /// Creates a new <see cref="EllipseLabelAnchorPoints"/>.
        /// </summary>
        /// <param name="circle">The circle.</param>
        protected EllipseLabelAnchorPoints()
        {
        }

        /// <inheritdoc />
        public override bool TryCalculate(IEllipseLabeled subject, string name, out LabelAnchorPoint value)
        {
            switch (name)
            {
                case "0":
                case "c":
                    value = new(subject.Center, new());
                    return true; // Center

                case "1":
                case "nw":
                    Vector2 pt = new(-subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    Vector2 n = new(pt.Y, pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n);
                    return true; // Top-left

                case "2":
                case "n":
                case "u":
                case "up":
                    value = new(subject.Center + new Vector2(0, -subject.RadiusY - subject.LabelMargin), new(0, -1));
                    return true; // Top

                case "3":
                case "ne":
                    pt = new(subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n);
                    return true; // Top-right

                case "4":
                case "e":
                case "r":
                case "right":
                    value = new(subject.Center + new Vector2(subject.RadiusX + subject.LabelMargin, 0), new(1, 0));
                    return true; // Right

                case "5":
                case "se":
                    pt = new(subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(pt.Y, pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n);
                    return true; // Bottom-right

                case "6":
                case "s":
                case "d":
                case "down":
                    value = new(subject.Center + new Vector2(0, subject.RadiusY + subject.LabelMargin), new(0, 1));
                    return true; // Bottom

                case "7":
                case "sw":
                    pt = new(-subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n);
                    return true; // Bottom-left

                case "8":
                case "w":
                case "l":
                case "left":
                    value = new(subject.Center + new Vector2(-subject.RadiusX - subject.LabelMargin, 0), new(-1, 0));
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
