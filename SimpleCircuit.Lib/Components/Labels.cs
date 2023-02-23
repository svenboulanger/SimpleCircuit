using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels
    {
        private readonly int _max;
        private readonly List<string> _labels;

        /// <summary>
        /// Gets or sets the label at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The label.</returns>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _labels.Count)
                    return null;
                return _labels[index];
            }
            set
            {
                if (index < 0 || index >= _max)
                    return;
                while (_labels.Count <= index)
                    _labels.Add(null);
                _labels[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of labels specified.
        /// </summary>
        public int Count => _labels.Count;

        /// <summary>
        /// Gets the maximum number of labels that can be supported.
        /// </summary>
        public int Maximum => _max;

        /// <summary>
        /// Creates a new <see cref="Labels"/> collection.
        /// </summary>
        /// <param name="maximum">The maximum.</param>
        public Labels(int maximum = 1)
        {
            _max = maximum;
            _labels = new();
        }
    }
}
