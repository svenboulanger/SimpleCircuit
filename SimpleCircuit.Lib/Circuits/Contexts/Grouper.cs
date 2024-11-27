using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Describes a class that can group items together.
    /// </summary>
    /// <typeparam name="K">The key.</typeparam>
    /// <typeparam name="V">The value.</typeparam>
    public abstract class Grouper<K, V> where V : IEquatable<V>
    {
        /// <summary>
        /// A group item.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="value">The value that describes how the item relates to the group.</param>
        protected readonly struct GroupItem(HashSet<K> group, V value)
        {
            /// <summary>
            /// Gets the group of keys in the same group.
            /// </summary>
            public HashSet<K> Group { get; } = group;

            /// <summary>
            /// Gets how the item links to the rest of the group.
            /// </summary>
            public V Value { get; } = value;
        }
        private readonly Dictionary<K, GroupItem> _dict;
        private readonly Dictionary<HashSet<K>, K> _representatives;

        /// <summary>
        /// Gets the comparer used for the grouper.
        /// </summary>
        public IEqualityComparer<K> Comparer { get; }

        /// <summary>
        /// Gets the representatives in the grouper.
        /// </summary>
        public IEnumerable<K> Representatives => _representatives.Values;

        /// <summary>
        /// Gets the number of groups.
        /// </summary>
        public int Count => _representatives.Count;

        /// <summary>
        /// Gets the number of nodes in any group.
        /// </summary>
        public int NodeCount => _dict.Count;

        /// <summary>
        /// Gets the link that describes a reference to itself.
        /// </summary>
        protected abstract V Self { get; }

        /// <summary>
        /// Creates a new <see cref="Grouper{K, V}"/>.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        protected Grouper(IEqualityComparer<K> comparer = null)
        {
            Comparer = comparer ?? EqualityComparer<K>.Default;
            _dict = new(Comparer);
            _representatives = [];
        }

        /// <summary>
        /// Gets the representative of the group that the key belongs to.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns the representative.</returns>
        public K GetRepresentative(K key)
        {
            if (_dict.TryGetValue(key, out var gi))
                return _representatives[gi.Group];
            return key;
        }

        /// <summary>
        /// Gets the value of the key relative to its group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public V GetValue(K key)
        {
            if (_dict.TryGetValue(key, out var gi))
                return gi.Value;
            return Self;
        }

        /// <summary>
        /// Tries to get both the representative and the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="representative">The representative.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGet(K key, out K representative, out V value)
        {
            if (_dict.TryGetValue(key, out var gi))
            {
                representative = _representatives[gi.Group];
                value = gi.Value;
                return true;
            }
            representative = key;
            value = Self;
            return false;
        }

        /// <summary>
        /// Adds a key if it doesn't exist yet in the grouper.
        /// </summary>
        /// <param name="a">The key.</param>
        public void Add(K a)
        {
            if (!_dict.ContainsKey(a))
            {
                GroupItem item = new([a], Self);
                _dict.Add(a, item);
                _representatives[item.Group] = a;
            }
        }

        /// <summary>
        /// Groups two nodes together.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        /// <param name="link">The link that says how <paramref name="b"/> links to <paramref name="a"/>.</param>
        /// <returns>Returns <c>true</c> if the item could be added; otherwise, <c>false</c>.</returns>
        public bool Group(K a, K b, V link)
        {
            if (Comparer.Equals(a, b))
            {
                if (_dict.TryGetValue(a, out var item))
                    return IsDuplicate(item, item, link);
                else if (link.Equals(Self))
                {
                    item = new([a], Self);
                    _dict.Add(a, item);
                    _representatives[item.Group] = a;
                    return true;
                }
                else
                    return false;
            }

            bool hasA = _dict.TryGetValue(a, out var ga);
            bool hasB = _dict.TryGetValue(b, out var gb);
            if (hasA && hasB)
            {
                // Already merged?
                if (ReferenceEquals(ga.Group, gb.Group))
                    return IsDuplicate(ga, gb, link);

                // Merge the two groups
                if (ga.Group.Count < gb.Group.Count)
                {
                    var invLink = Invert(link);
                    foreach (var n in ga.Group)
                    {
                        var current = _dict[n].Value;
                        _dict[n] = new(gb.Group, MoveLink(gb.Value, ga.Value, current, invLink));
                    }
                    gb.Group.UnionWith(ga.Group);
                    _representatives.Remove(ga.Group);
                }
                else
                {
                    foreach (var n in gb.Group)
                    {
                        var current = _dict[n].Value;
                        _dict[n] = new(ga.Group, MoveLink(ga.Value, gb.Value, current, link));
                    }
                    ga.Group.UnionWith(gb.Group);
                    _representatives.Remove(gb.Group);
                }
            }
            else if (hasA && !hasB)
            {
                _dict[b] = new(ga.Group, NewLink(ga.Value, link));
                ga.Group.Add(b);
            }
            else if (hasB && !hasA)
            {
                _dict[a] = new(gb.Group, NewLink(gb.Value, Invert(link)));
                gb.Group.Add(a);
            }
            else
            {
                ga = new([a, b], Self);
                _dict.Add(a, ga);
                _dict.Add(b, new(ga.Group, link));
                _representatives[ga.Group] = a;
            }
            return true;
        }

        /// <summary>
        /// Determines whether two keys belong to the same group.
        /// </summary>
        /// <param name="a">The first key.</param>
        /// <param name="b">The second key.</param>
        /// <returns>Returns <c>true</c> if both are grouped; otherwise, <c>false</c>.</returns>
        public bool AreGrouped(K a, K b)
        {
            if (_dict.TryGetValue(a, out var ga) &&
                _dict.TryGetValue(b, out var gb))
                return ReferenceEquals(ga.Group, gb.Group);
            return false;
        }

        /// <summary>
        /// Determines whether the link between <paramref name="a"/> and <paramref name="b"/> is 
        /// given by <paramref name="link"/>.
        /// </summary>
        /// <param name="a">The first group item.</param>
        /// <param name="b">The second group item.</param>
        /// <param name="link">The link to be checked.</param>
        /// <returns>Returns <c>true</c> if the link represents the same link as the one that already exists.</returns>
        protected abstract bool IsDuplicate(GroupItem a, GroupItem b, V link);

        /// <summary>
        /// Inverts the link. If A is linked to B by link, then the result should
        /// be how B is linked to A.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>Returns the inverted link.</returns>
        protected abstract V Invert(V link);

        /// <summary>
        /// Calculates the new link when two groups are being merged.
        /// </summary>
        /// <param name="linkReference">The value of the reference item that will stay around.</param>
        /// <param name="linkMerged">The value of the item that will be removed after merging.</param>
        /// <param name="linkCurrent">The value of the item that is currently being moved.</param>
        /// <param name="link">The link that was added between <paramref name="linkMerged"/> and <paramref name="linkReference"/>.</param>
        /// <returns>Returns the new link of the item represented by <paramref name="linkCurrent"/>.</returns>
        protected abstract V MoveLink(V linkReference, V linkMerged, V linkCurrent, V link);

        /// <summary>
        /// Finds the value of a link when a new item would be created.
        /// </summary>
        /// <param name="linkReference">The reference item value.</param>
        /// <param name="link">The created link.</param>
        /// <returns>Returns the link of the item.</returns>
        protected abstract V NewLink(V linkReference, V link);
    }
}
