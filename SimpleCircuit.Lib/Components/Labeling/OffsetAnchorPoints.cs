using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Applies an offset to existing label anchor points.
    /// </summary>
    /// <typeparam name="T">The subject type.</typeparam>
    public class OffsetAnchorPoints<T> : LabelAnchorPoints<T> where T : ILabeled
    {
        private readonly ILabelAnchorPoints<T> _anchors;
        private readonly int _offset;

        /// <inheritdoc />
        public override int Count => _anchors.Count;

        /// <summary>
        /// Creates a new <see cref="OffsetAnchorPoints{T}"/>.
        /// </summary>
        /// <param name="anchors">The anchors.</param>
        /// <param name="offset">The offset.</param>
        public OffsetAnchorPoints(ILabelAnchorPoints<T> anchors, int offset)
        {
            _anchors = anchors ?? throw new ArgumentNullException(nameof(anchors));
            _offset = offset;
        }

        /// <inheritdoc />
        public override bool TryCalculate(T subject, string name, out LabelAnchorPoint value)
        {
            if (name.All(char.IsDigit))
            {
                int index = int.Parse(name);
                return _anchors.TryCalculate(subject, (index + _offset).ToString(), out value);
            }
            else
                return _anchors.TryCalculate(subject, name, out value);
        }
    }
}
