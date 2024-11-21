using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// The aggregate of two lists of label anchor points.
    /// </summary>
    /// <remarks>
    /// Creates a new aggregate anchor points list.
    /// </remarks>
    /// <param name="a">The first list.</param>
    /// <param name="b">The second list.</param>
    public class AggregateAnchorPoints<T>(ILabelAnchorPoints<T> a, ILabelAnchorPoints<T> b) : LabelAnchorPoints<T> where T : IDrawable
    {
        private readonly ILabelAnchorPoints<T> _a = a, _b = b;

        /// <inheritdoc />
        public override int Count => _a.Count + _b.Count;

        /// <inheritdoc />
        public override bool TryCalculate(T subject, string name, out LabelAnchorPoint value)
        {
            if (name.All(char.IsDigit))
            {
                int index = int.Parse(name);
                index %= Count;
                if (index < 0)
                    index += Count;
                if (index < _a.Count)
                    return _a.TryCalculate(subject, index.ToString(), out value);
                else
                    return _b.TryCalculate(subject, (index - _a.Count).ToString(), out value);
            }

            // Just try in order
            return _a.TryCalculate(subject, name, out value) || _b.TryCalculate(subject, name, out value);
        }
    }
}
