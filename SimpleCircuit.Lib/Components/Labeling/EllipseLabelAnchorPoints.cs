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
        public override LabelAnchorPoint Calculate(IEllipseLabeled subject, int index)
        {
            index %= Count;
            if (index < 0)
                index += Count;

            switch (index)
            {
                case 0: return new(subject.Center, new()); // Center
                case 1:
                    Vector2 pt = new(-subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    Vector2 n = new(pt.Y, pt.X);
                    n /= n.Length;
                    return new(subject.Center + pt + subject.LabelMargin * n, n); // Top-left
                case 2:
                    return new(subject.Center + new Vector2(0, -subject.RadiusY - subject.LabelMargin), new(0, -1)); // Top
                case 3:
                    pt = new(subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    return new(subject.Center + pt + subject.LabelMargin * n, n); // Top-right
                case 4:
                    return new(subject.Center + new Vector2(subject.RadiusX + subject.LabelMargin, 0), new(1, 0)); // Right
                case 5:
                    pt = new(subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(pt.Y, pt.X);
                    n /= n.Length;
                    return new(subject.Center + pt + subject.LabelMargin * n, n); // Bottom-right
                case 6:
                    return new(subject.Center + new Vector2(0, subject.RadiusY + subject.LabelMargin), new(0, 1)); // Bottom
                case 7:
                    pt = new(-subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    return new(subject.Center + pt + subject.LabelMargin * n, n); // Bottom-left
                case 8:
                    return new(subject.Center + new Vector2(-subject.RadiusX - subject.LabelMargin, 0), new(-1, 0)); // Left
            }
            return new();
        }
    }
}
