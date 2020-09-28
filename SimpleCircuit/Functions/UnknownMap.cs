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
        /// <returns></returns>
        public int Map(Unknown unknown)
        {
            if (!_unknowns.TryGetValue(unknown, out var index))
            {
                index = _unknowns.Count + 1;
                _unknowns.Add(unknown, index);
            }
            return index;
        }

        public bool TryGet(Unknown unknown, out int index) => _unknowns.TryGetValue(unknown, out index);

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

        public IEnumerator<KeyValuePair<Unknown, int>> GetEnumerator()
            => _unknowns.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
