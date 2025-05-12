using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Labeling
{

    /// <summary>
    /// A list of default placements for labels.
    /// </summary>
    public abstract class LabelAnchorPoints<T> : ILabelAnchorPoints<T> where T : IDrawable
    {
        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract bool TryCalculate(T subject, string name, out LabelAnchorPoint value);

        /// <inheritdoc />
        public void Draw(IGraphicsBuilder builder, T subject)
        {
            for (int i = 0; i < subject.Labels.Count; i++)
            {
                // Get the label
                var label = subject.Labels[i];
                if (label is null)
                    continue;

                // Get the anchor point
                if (!TryCalculate(subject, label.Location ?? i.ToString(), out var anchor))
                    TryCalculate(subject, "0", out anchor); // Default to index 0

                // Determine the final values
                var location = anchor.Location;
                if (!label.Offset.IsZero())
                    location += builder.CurrentTransform.Matrix.Inverse * label.Offset;
                var expand = label.Expand ?? anchor.Expand;

                // Draw the label
                if (label.Formatted is null)
                    builder.Text(label.Value, location, expand, anchor.Appearance, anchor.Oriented);
                else
                    builder.Text(label.Formatted, location, expand, anchor.Oriented);
            }
        }
    }
}
