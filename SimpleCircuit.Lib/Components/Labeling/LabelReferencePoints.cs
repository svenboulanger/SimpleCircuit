using System.Collections.Generic;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of default placements for labels.
    /// </summary>
    public abstract class LabelAnchorPoints<T>
    {
        /// <summary>
        /// Gets the number of anchor points defined.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Calculates the anchor point at the given index for the subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="index">The index.</param>
        /// <returns>The label anchor point.</returns>
        public abstract LabelAnchorPoint Calculate(T subject, int index);

        /// <summary>
        /// Draws labels using the anchor points
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="labels">The labels.</param>
        /// <param name="subject">The subject.</param>
        public void Draw(SvgDrawing drawing, IReadOnlyList<LabelInfo> labels, T subject)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                // Get the label
                var label = labels[i];
                if (label == null)
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
