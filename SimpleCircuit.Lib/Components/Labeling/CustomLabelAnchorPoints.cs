using System;
using System.Linq;
using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of custom label anchor points.
    /// </summary>
    /// <remarks>
    /// Creates a new list of anchor points.
    /// </remarks>
    /// <param name="points">The points.</param>
    public class CustomLabelAnchorPoints(params LabelAnchorPoint[] points) : LabelAnchorPoints<IDrawable>
    {
        private readonly LabelAnchorPoint[] _points = points ?? throw new ArgumentNullException(nameof(points));

        /// <summary>
        /// Gets or sets the label anchor point at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The label anchor point.</returns>
        public LabelAnchorPoint this[int index]
        {
            get => _points[index];
            set => _points[index] = value;
        }

        /// <inheritdoc />
        public override int Count => _points.Length;

        /// <inheritdoc />
        public void Draw(IGraphicsBuilder drawing, Labels labels)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                // Get the label
                var label = labels[i];
                if (label is null)
                    continue;

                // Get the anchor point
                if (!TryCalculate(label.Location ?? i.ToString(), out var anchor))
                    TryCalculate("0", out anchor); // Default to index 0

                // Determine the final values
                var location = anchor.Location;
                if (!label.Offset.IsZero())
                    location += drawing.CurrentTransform.Matrix.Inverse * label.Offset;
                var expand = label.Expand ?? anchor.Expand;

                // Draw the label
                drawing.Text(label.Value, location, expand, anchor.Appearance, anchor.Oriented);
            }
        }

        /// <summary>
        /// Tries to calculate the index of the anchor point based on the anchor name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="index">The index.</param>
        /// <returns>Returns <c>true</c> if the index could be calculated; otherwise, <c>false</c>.</returns>
        public bool TryCalculateIndex(string name, out int index)
        {
            if (name.All(char.IsDigit))
            {
                index = int.Parse(name);
                index %= Count;
                if (index < 0)
                    index += Count;
                return true;
            }
            index = -1;
            return false;
        }

        /// <inheritdoc />
        public override bool TryCalculate(IDrawable subject, string name, out LabelAnchorPoint value)
            => TryCalculate(name, out value);

        /// <summary>
        /// Tries to calculate the label anchor point just based on index.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if the label anchor point was found; otherwise, <c>false</c>.</returns>
        protected bool TryCalculate(string name, out LabelAnchorPoint value)
        {
            if (TryCalculateIndex(name, out int index))
            {
                value = _points[index];
                return true;
            }
            value = default;
            return false;
        }
    }
}
