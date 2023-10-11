namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of label anchor points that can be used for a box.
    /// </summary>
    public class BoxLabelAnchorPoints : LabelAnchorPoints<IBoxLabeled>
    {
        /// <summary>
        /// Gets the default box label anchor points.
        /// </summary>
        public static BoxLabelAnchorPoints Default { get; } = new BoxLabelAnchorPoints();

        /// <inheritdoc />
        public override int Count => 21;

        /// <summary>
        /// Creates a new <see cref="BoxLabelAnchorPoints"/>.
        /// </summary>
        protected BoxLabelAnchorPoints()
        {
        }

        /// <inheritdoc />
        public override LabelAnchorPoint Calculate(IBoxLabeled subject, int index)
        {
            // Normalize the index
            index %= Count;
            if (index < 0)
                index += Count;

            switch (index % Count)
            {
                case 0: return new(0.5 * (subject.TopLeft + subject.BottomRight), new()); // Center

                case 1: return new(subject.TopLeft + new Vector2(subject.CornerRadius, -subject.LabelMargin), new(1, -1)); // Top-left above box
                case 2: return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y - subject.LabelMargin), new(0, -1)); // Top center above box
                case 3: return new(new(subject.BottomRight.X - subject.CornerRadius, subject.TopLeft.Y - subject.LabelMargin), new(-1, -1)); // Top-right above box

                case 4: return new(new(subject.BottomRight.X + subject.LabelMargin, subject.TopLeft.Y + subject.CornerRadius), new(1, 1)); // Top-right right of box
                case 5: return new(new(subject.BottomRight.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0)); // Middle-right right of box
                case 6: return new(subject.BottomRight + new Vector2(subject.LabelMargin, -subject.CornerRadius), new(1, -1)); // Bottom-right right of box

                case 7: return new(subject.BottomRight + new Vector2(-subject.CornerRadius, subject.LabelMargin), new(-1, 1)); // Bottom-right below box
                case 8: return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y + subject.LabelMargin), new(0, 1)); // Bottom-center below box
                case 9: return new(new(subject.TopLeft.X + subject.CornerRadius, subject.BottomRight.Y + subject.LabelMargin), new(1, 1)); // Bottom-right below box

                case 10: return new(new(subject.TopLeft.X - subject.LabelMargin, subject.BottomRight.Y - subject.CornerRadius), new(-1, -1)); // Bottom-left left of box
                case 11: return new(new(subject.TopLeft.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0)); // Middle-left left of box
                case 12: return new(subject.TopLeft + new Vector2(-subject.LabelMargin, subject.CornerRadius), new(-1, 1)); // Top-left left of box

                case 13:
                    double f = subject.CornerRadius * 0.70710678118;
                    return new(subject.TopLeft + new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(1, 1)); // Top-left inside box
                case 14:
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y + subject.LabelMargin), new(0, 1)); // Top-center inside box
                case 15:
                    f = subject.CornerRadius * 0.70710678118;
                    return new(new(subject.BottomRight.X - f - subject.LabelMargin, subject.TopLeft.Y + f + subject.LabelMargin), new(-1, 1)); // Top-right inside box
                case 16:
                    return new(new(subject.BottomRight.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0)); // Middle-right inside box
                case 17:
                    f = subject.CornerRadius * 0.70710678118;
                    return new(subject.BottomRight - new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(-1, -1)); // Bottom-right inside box
                case 18:
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y - subject.LabelMargin), new(0, -1)); // Bottom-center inside box
                case 19:
                    f = subject.CornerRadius * 0.70710678118;
                    return new(new(subject.TopLeft.X + f + subject.LabelMargin, subject.BottomRight.Y - f - subject.LabelMargin), new(1, -1)); // Bottom-left inside box
                case 20:
                    return new(new(subject.TopLeft.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0)); // Middle-left inside box
            }
            return new();
        }
    }
}
