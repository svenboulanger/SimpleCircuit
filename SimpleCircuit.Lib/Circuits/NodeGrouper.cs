using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits
{
    /// <summary>
    /// A class for grouping nodes together.
    /// It will also make sure ground nodes are not mapped to other nodes.
    /// </summary>
    public class NodeGrouper
    {
        /// <summary>
        /// The ground nodes.
        /// </summary>
        public static string[] _ground = new[] { "0", "gnd", "gnd!" };

        private struct NodeGroup
        {
            public string Representative { get; }
            public HashSet<string> Nodes { get; }
            public NodeGroup(string a)
            {
                Representative = a ?? throw new ArgumentNullException(nameof(a));
                Nodes = new(StringComparer.OrdinalIgnoreCase) { a };
            }
            public NodeGroup(string a, string b)
            {
                Representative = a ?? throw new ArgumentNullException(nameof(a));
                Nodes = new(StringComparer.OrdinalIgnoreCase) { a, b };
            }
        }
        private readonly NodeGroup _gndGroup;
        private readonly Dictionary<string, NodeGroup> _dict = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets all the representatives in the grouper.
        /// </summary>
        public IEnumerable<string> Representatives
        {
            get
            {
                HashSet<string> _done = new(StringComparer.OrdinalIgnoreCase);
                foreach (var group in _dict.Values)
                {
                    if (_done.Add(group.Representative))
                        yield return group.Representative;
                }
            }
        }

        /// <summary>
        /// Gets the representative for a nodes.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>The representative of the node.</returns>
        public string this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (_dict.TryGetValue(name, out var group))
                    return group.Representative;
                return name;
            }
        }

        /// <summary>
        /// Creates a new node grouper.
        /// </summary>
        public NodeGrouper()
        {
            bool first = true;
            NodeGroup g = new();
            foreach (string node in _ground)
            {
                if (first)
                {
                    g = new(node);
                    first = false;
                }
                g.Nodes.Add(node);
                _dict.Add(node, g);
            }
            _gndGroup = g;
        }

        /// <summary>
        /// Groups two nodes together.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        public void Group(string a, string b)
        {
            bool hasA = _dict.TryGetValue(a, out var ga);
            bool hasB = _dict.TryGetValue(b, out var gb);
            if (hasA && hasB)
            {
                // Already merged?
                if (ga.Representative == gb.Representative)
                    return;

                // Merge the two groups
                if (ga.Nodes.Count < gb.Nodes.Count && !_gndGroup.Nodes.Contains(a))
                {
                    foreach (var n in ga.Nodes)
                        _dict[n] = gb;
                    gb.Nodes.UnionWith(ga.Nodes);
                }
                else
                {
                    foreach (var n in gb.Nodes)
                        _dict[n] = ga;
                    ga.Nodes.UnionWith(gb.Nodes);
                }
            }
            else if (hasA && !hasB)
            {
                _dict[b] = ga;
                ga.Nodes.Add(b);
            }
            else if (hasB && !hasA)
            {
                _dict[a] = gb;
                gb.Nodes.Add(a);
            }
            else
            {
                var g = new NodeGroup(a, b);
                _dict.Add(a, g);
                _dict.Add(b, g);
            }
        }

        /// <summary>
        /// Returns whether two nodes are grouped together.
        /// </summary>
        /// <param name="a">The first node.</param>
        /// <param name="b">The second node.</param>
        /// <returns>Returns <c>true</c> if they are grouped; otherwise, <c>false</c>.</returns>
        public bool AreGrouped(string a, string b)
            => this[a] == this[b];
    }
}
