using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Styles;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels : IReadOnlyList<Label>
    {
        private readonly List<Label> _labels = [];

        /// <inheritdoc />
        public int Count => _labels.Count;

        /// <inheritdoc />
        public Label this[int index]
        {
            get
            {
                if (index < 0)
                    return null;

                // Pad with more labels if necessary
                if (index >= _labels.Count)
                {
                    for (int i = _labels.Count; i <= index; i++)
                        _labels.Add(null);
                }

                // Create a new label if it doesn't exist yet
                Label result;
                if (_labels[index] == null)
                {
                    result = new Label();
                    _labels[index] = result;
                }
                else
                    result = _labels[index];
                return result;
            }
        }

        /// <summary>
        /// Formats all labels.
        /// </summary>
        /// <param name="formatter">The text formatter.</param>
        /// <param name="parentStyle">The parent style.</param>
        public void Format(ITextFormatter formatter, IStyle parentStyle)
        {
            foreach (var label in _labels)
                label?.Format(formatter, parentStyle);
        }

        /// <inheritdoc />
        public IEnumerator<Label> GetEnumerator() => _labels.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
