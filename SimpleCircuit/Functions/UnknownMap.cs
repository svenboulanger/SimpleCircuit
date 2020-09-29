using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// Maps unknowns to indices.
    /// </summary>
    public class UnknownMap : IEnumerable<KeyValuePair<Unknown, int>>
    {
        private Dictionary<Unknown, int> _unknowns = new Dictionary<Unknown, int>();

        /// <summary>
        /// Gets the number of unknowns.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _unknowns.Count;

        /// <summary>
        /// Maps the specified unknown.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <returns>The index of the unknown.</returns>
        public int Map(Unknown unknown)
        {
            if (!_unknowns.TryGetValue(unknown, out var index))
            {
                index = _unknowns.Count + 1;
                _unknowns.Add(unknown, index);
            }
            return index;
        }

        /// <summary>
        /// Tries to get the index associated with the specified unknown.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///     <c>true</c> if the unknown is mapped; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGet(Unknown unknown, out int index) => _unknowns.TryGetValue(unknown, out index);

        /// <summary>
        /// Gets the unknown associated with the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The unknown.</returns>
        public Unknown Reverse(int index)
        {
            return _unknowns.FirstOrDefault(p => p.Value == index).Key;
        }

        /// <summary>
        /// Clears all unknowns.
        /// </summary>
        public void Clear()
        {
            _unknowns.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Unknown, int>> GetEnumerator()
            => _unknowns.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
