using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A class for managing the labels of a component.
    /// </summary>
    public class Labels
    {
        private readonly string[] _labels;

        /// <summary>
        /// Gets or sets the label at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The label.</returns>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _labels.Length)
                    return null;
                return _labels[index];
            }
            set
            {
                if (index < 0 || index >= _labels.Length)
                    return;
                _labels[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of labels supported.
        /// </summary>
        public int Count => _labels.Length;

        /// <summary>
        /// Creates a new <see cref="Labels"/> collection.
        /// </summary>
        /// <param name="maximum">The maximum.</param>
        public Labels(int maximum = 1)
        {
            if (maximum < 1)
                throw new ArgumentOutOfRangeException(nameof(maximum));
            _labels = new string[maximum];
        }
    }
}
