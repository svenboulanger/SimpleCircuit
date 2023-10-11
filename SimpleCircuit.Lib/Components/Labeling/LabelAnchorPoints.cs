namespace SimpleCircuit.Components.Labeling
{

    /// <summary>
    /// A list of default placements for labels.
    /// </summary>
    public abstract class LabelAnchorPoints<T> : ILabelAnchorPoints<T> where T : ILabeled
    {
        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract LabelAnchorPoint Calculate(T subject, int index);

        /// <inheritdoc />
        public void Draw(SvgDrawing drawing, T subject)
        {
            for (int i = 0; i < subject.Labels.Count; i++)
            {
                // Get the label
                var label = subject.Labels[i];
                if (label is null)
                    continue;

                // Get the anchor point
                var anchor = Calculate(subject, label.Location ?? i);

                // Determine the final values
                var location = anchor.Location + label.Offset;
                var expand = label.Expand ?? anchor.Expand;

                // Draw the label
                drawing.Text(label.Value, location, expand, anchor.Options);
            }
        }
    }
}
