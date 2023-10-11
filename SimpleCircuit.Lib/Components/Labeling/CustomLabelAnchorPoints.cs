using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of custom label anchor points.
    /// </summary>
    public class CustomLabelAnchorPoints : LabelAnchorPoints<ILabeled>
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
        /// Creates a new list of anchor points.
        /// </summary>
        /// <param name="points">The points.</param>
        public CustomLabelAnchorPoints(params LabelAnchorPoint[] points)
        {
            _points = points ?? throw new ArgumentNullException(nameof(points));
        }

        /// <inheritdoc />
        public override bool TryCalculate(ILabeled subject, string name, out LabelAnchorPoint value)
        {
            if (name.All(char.IsDigit))
            {
                int index = int.Parse(name);
                index %= Count;
                if (index < 0)
                    index += Count;
                value = _points[index];
                return true;
            }

            value = default;
            return false;
        }
    }
}
