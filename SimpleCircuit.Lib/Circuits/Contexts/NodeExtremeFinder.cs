using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A class for tracking an extreme of a group of nodes.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="NodeExtremeFinder"/>.
    /// </remarks>
    /// <param name="comparer">The comparer. The default is a case-insensitive comparer.</param>
    public class NodeExtremeFinder(IEqualityComparer<string> comparer = null)
    {
        private readonly HashSet<string> _extremes = new(comparer ?? StringComparer.OrdinalIgnoreCase),
            _nonExtremes = new(comparer ?? StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets all the extremes tracked by the sorter.
        /// </summary>
        public IEnumerable<string> Extremes => _extremes;

        /// <summary>
        /// Tracks an order between nodes.
        /// </summary>
        /// <param name="extreme">The extreme node.</param>
        /// <param name="nonExtreme">The non-extreme node.</param>
        public void Order(string extreme, string nonExtreme)
        {
            // The non-extreme node becomes non-extreme
            _extremes.Remove(nonExtreme);

            // Make sure the non-extreme node is being tracked
            _nonExtremes.Add(nonExtreme);

            // Add the extreme if it wasn't handled already
            if (!_nonExtremes.Contains(extreme))
                _extremes.Add(extreme);
        }

        /// <summary>
        /// Clears the sorter.
        /// </summary>
        public void Clear()
        {
            _extremes.Clear();
            _nonExtremes.Clear();
        }

        /// <summary>
        /// Converts the class to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => $"Extreme finder ({_extremes.Count}/{_nonExtremes.Count + _extremes.Count})";
    }
}
