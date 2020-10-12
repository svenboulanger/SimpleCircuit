using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// This is a 26-branch tree-based structure that allows searching for keywords. It will always try to
    /// find the longest first letters.
    /// </summary>
    public class KeySearch<V>
    {
        private class Node
        {
            public Node[] Next { get; set; }
            public V Result { get; set; }
            public bool IsSet { get; set; }
        }
        private Node _root = new Node();

        /// <summary>
        /// Gets the number of factories.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Adds a value for the specified search key string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, V value)
        {
            var current = _root;
            for (var i = 0; i < key.Length; i++)
            {
                var c = char.ToLower(key[i]);
                if (c < 'a' || c > 'z')
                    throw new ArgumentException($"Cannot add {key} to the search tree because it doesn't use only normal letters.");
                if (current.Next == null)
                    current.Next = new Node[26];
                if (current.Next[c - 'a'] == null)
                {
                    var n = new Node();
                    current.Next[c - 'a'] = n;
                    current = n;
                }
                else
                    current = current.Next[c - 'a'];
            }
            if (current.Result != null)
                throw new ArgumentException($"There is already a value for {key}");
            current.Result = value;
            current.IsSet = true;
            Count++;
        }

        /// <summary>
        /// Gets the match.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the key is exactly present in this instance; otherwise, <c>false</c>.</returns>
        public bool Search(string key, out V value)
        {
            value = default;
            var current = _root;
            for (var i = 0; i < key.Length; i++)
            {
                var c = char.ToLower(key[i]);
                if (c < 'a' || c > 'z')
                    return false;
                if (current.Next != null && current.Next[c - 'a'] != null)
                {
                    current = current.Next[c - 'a'];
                    value = current.IsSet ? current.Result : value;
                }
                else
                    return false;
            }

            // We reached the end of the key!
            // If the current node is also a set value, then we have an exact match!
            return current.IsSet;
        }

        /// <summary>
        /// Clears the keyword search.
        /// </summary>
        public void Clear()
        {
            _root = new Node();
        }
    }
}
