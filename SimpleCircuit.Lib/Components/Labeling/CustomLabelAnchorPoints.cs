using System;
using System.Linq;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of custom label anchor points.
    /// </summary>
    public class CustomLabelAnchorPoints : LabelAnchorPoints<IDrawable>
    {
        private readonly LabelAnchorPoint[] _points;

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

        /// <summary>
        /// Creates a new <see cref="CustomLabelAnchorPoints"/>.
        /// </summary>
        /// <param name="points">The points.</param>
        public CustomLabelAnchorPoints(params LabelAnchorPoint[] points)
        {
            _points = points ?? throw new ArgumentNullException(nameof(points));
        }

        /// <summary>
        /// Creates a new <see cref="CustomLabelAnchorPoints"/>.
        /// </summary>
        /// <param name="count">The number of label anchors.</param>
        public CustomLabelAnchorPoints(int count)
        {
            _points = new LabelAnchorPoint[count];
        }

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
        /// Calculates the bounds of all potentially overlapping labels at a given anchor.
        /// </summary>
        /// <param name="labels">The labels.</param>
        /// <param name="anchorIndex">The anchor index.</param>
        /// <returns>The bounds when all labels at the given anchor overlap.</returns>
        public Bounds CalculateBounds(Labels labels, int anchorIndex)
        {
            var bounds = new ExpandableBounds();
            for (int i = 0; i < labels.Count; i++)
            {
                string name = labels[i].Location ?? i.ToString();
                if (!TryCalculateIndex(name, out int index) || index != anchorIndex)
                    continue;
                bounds.Expand(labels[i].Formatted.Bounds.Bounds);
            }
            if (double.IsInfinity(bounds.Bounds.Width))
                bounds.Expand(new Vector2());
            return bounds.Bounds;
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
