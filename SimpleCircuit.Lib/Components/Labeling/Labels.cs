using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels : IReadOnlyList<LabelInfo>
    {
        private readonly List<LabelInfo> _labels = new();

        /// <inheritdoc />
        public LabelInfo this[int index]
        {
            get
            {
                if (index < 0)
                    return null;
                if (index >= _labels.Count)
                {
                    for (int i = _labels.Count; i <= index; i++)
                        _labels.Add(null);
                }

                LabelInfo result;
                if (_labels[index] == null)
                {
                    result = new LabelInfo();
                    _labels[index] = result;
                }
                else
                    result = _labels[index];
                return result;
            }
        }

        /// <inheritdoc />
        public int Count => _labels.Count;

        /// <inheritdoc />
        public IEnumerator<LabelInfo> GetEnumerator() => _labels.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
