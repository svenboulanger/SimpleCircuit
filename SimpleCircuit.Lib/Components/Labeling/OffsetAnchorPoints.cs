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
