using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Applies an offset to existing label anchor points.
    /// </summary>
    /// <typeparam name="T">The subject type.</typeparam>
    /// <remarks>
    /// Creates a new <see cref="OffsetAnchorPoints{T}"/>.
    /// </remarks>
    /// <param name="anchors">The anchors.</param>
    /// <param name="offset">The offset.</param>
    public class OffsetAnchorPoints<T>(ILabelAnchorPoints<T> anchors, int offset) : LabelAnchorPoints<T> where T : IDrawable
    {
        private readonly ILabelAnchorPoints<T> _anchors = anchors ?? throw new ArgumentNullException(nameof(anchors));
        private readonly int _offset = offset;

        /// <inheritdoc />
        public override int Count => _anchors.Count;

        /// <inheritdoc />
        public override bool TryGetAnchorIndex(string name, out int index)
        {
            if (name.All(char.IsDigit))
            {
                index = int.Parse(name);
                index = index + _offset;
                index %= Count;
                if (index < 0)
                    index += Count;
                return true;
            }
            else
                return _anchors.TryGetAnchorIndex(name, out index);
        }

        /// <inheritdoc />
        public override LabelAnchorPoint GetAnchorPoint(T subject, int index)
            => _anchors.GetAnchorPoint(subject, index);
    }
}
