namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// The aggregate of two lists of label anchor points.
    /// </summary>
    public class AggregateAnchorPoints<T> : LabelAnchorPoints<T> where T : ILabeled
    {
        private readonly ILabelAnchorPoints<T> _a, _b;

        /// <inheritdoc />
        public override int Count => _a.Count + _b.Count;

        /// <summary>
        /// Creates a new aggregate anchor points list.
        /// </summary>
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        public AggregateAnchorPoints(ILabelAnchorPoints<T> a, ILabelAnchorPoints<T> b)
        {
            _a = a;
            _b = b;
        }

        /// <inheritdoc />
        public override LabelAnchorPoint Calculate(T subject, int index)
        {
            index %= Count;
            if (index < 0)
                index += Count;

            if (index < _a.Count)
                return _a.Calculate(subject, index);
            else
                return _b.Calculate(subject, index - _a.Count);
        }
    }
}
