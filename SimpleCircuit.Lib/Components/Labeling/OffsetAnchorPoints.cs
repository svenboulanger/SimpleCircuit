using System;

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
        public override LabelAnchorPoint Calculate(T subject, int index)
            => _anchors.Calculate(subject, index + _offset);
    }
}
