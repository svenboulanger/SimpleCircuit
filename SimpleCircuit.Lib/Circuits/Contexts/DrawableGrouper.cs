using SimpleCircuit.Components;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Allows grouping drawables together.
    /// </summary>
    public class DrawableGrouper
    {
        public readonly struct Key(string groupX, string groupY) : IEquatable<Key>
        {
            public string GroupX { get; } = groupX;
            public string GroupY { get; } = groupY;
            public bool Equals(Key other)
                => StringComparer.Ordinal.Equals(GroupX, other.GroupX) && StringComparer.Ordinal.Equals(GroupY, other.GroupY);
        }

        public readonly struct GroupData()
        {
            public HashSet<IDrawable> Drawables { get; } = [];
            public HashSet<string> RepresentativesX { get; } = [];
            public HashSet<string> RepresentativesY { get; } = [];
        }

        private readonly Dictionary<Key, GroupData> _dict = [];

        /// <summary>
        /// Gets the number of groups.
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        /// Gets a set of key-value pairs with (X, Y) groups and the group data.
        /// </summary>
        public IEnumerable<KeyValuePair<Key, GroupData>> Groups => _dict;

        /// <summary>
        /// Gets the group data for the given key.
        /// </summary>
        /// <param name="groupX">The group coordinate X.</param>
        /// <param name="groupY">The group coordinate Y.</param>
        /// <returns>Returns the group data.</returns>
        public GroupData this[string groupX, string groupY] => _dict[new(groupX, groupY)];

        /// <summary>
        /// Gets the group data for the given key.
        /// </summary>
        /// <param name="key">The group key.</param>
        /// <returns>Returns the group data.</returns>
        public GroupData this[Key key] => _dict[key];

        /// <summary>
        /// Group a drawable with its group while storing the drawable and the representatives.
        /// </summary>
        /// <param name="drawable">The drawables.</param>
        /// <param name="groupX">The group X.</param>
        /// <param name="groupY">The group Y.</param>
        /// <param name="repX">The representative for the X-coordinate.</param>
        /// <param name="repY">The representative for the Y-coordinate.</param>
        public void Group(IDrawable drawable, string groupX, string groupY, string repX, string repY)
        {
            var key = new Key(groupX, groupY);
            if (!_dict.TryGetValue(key, out var data))
            {
                data = new();
                _dict.Add(key, data);
            }
            data.Drawables.Add(drawable);
            data.RepresentativesX.Add(repX);
            data.RepresentativesY.Add(repY);
        }

        /// <summary>
        /// Tries to get a value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        /// <returns>Returns <c>true</c> if the group exists; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(Key key, out GroupData data) => _dict.TryGetValue(key, out data);
    }
}
