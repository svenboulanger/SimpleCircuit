using System;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A class for grouping nodes together.
    /// It will also make sure ground nodes are not mapped to other nodes.
    /// </summary>
    public class NodeOffsetFinder
    {
        private bool _found = true;

        /// <summary>
        /// The ground nodes.
        /// </summary>
        public static string[] _ground = ["0", "gnd", "gnd!"];

        private readonly struct NodeGroupItem(NodeGroup group, double offset)
        {
            public NodeGroup Group { get; } = group;
            public double Offset { get; } = offset;
            public override string ToString() => $"{Group.Representative} + {Offset:G3}";
        }
        private readonly struct NodeGroup
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
        private readonly Dictionary<string, NodeGroupItem> _dict = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets whether new offsets have been found. If assigned false, the found flag is reset.
        /// </summary>
        public bool Found
        {
            get => _found;
            set => _found &= value;
        }

        /// <summary>
        /// Gets all the representatives in the grouper.
        /// </summary>
        public IEnumerable<string> Representatives
        {
            get
            {
                if (_dict.Count > 0)
                {
                    var done = new HashSet<string>(_dict.Comparer);
                    foreach (var value in _dict.Values)
                    {
                        if (done.Add(value.Group.Representative))
                            yield return value.Group.Representative;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of representatives in the circuit.
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        /// Gets the representative for a nodes.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>The representative of the node.</returns>
        public RelativeItem this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                if (_dict.TryGetValue(name, out var item))
                    return new(item.Group.Representative, item.Offset);
                return new(name, 0.0);
            }
        }

        /// <summary>
        /// Creates a new node grouper.
        /// </summary>
        public NodeOffsetFinder()
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
                _dict.Add(node, new(g, 0.0));
            }
            _gndGroup = g;
        }

        /// <summary>
        /// Groups two nodes together.
        /// </summary>
        /// <param name="lowest">The first node.</param>
        /// <param name="highest">The second node.</param>
        /// <returns>
        ///     Returns <c>true</c> if the grouping was succesful; otherwise, <c>false</c>.
        /// </returns>
        public bool Group(string lowest, string highest, double offset)
        {
            bool hasA = _dict.TryGetValue(lowest, out var itemA);
            bool hasB = _dict.TryGetValue(highest, out var itemB);
            if (hasA && hasB)
            {
                // Already merged?
                if (itemA.Group.Representative == itemB.Group.Representative)
                    return (itemB.Offset - itemA.Offset - offset).IsZero();

                // Merge the two groups
                _found = true;
                if (itemA.Group.Nodes.Count < itemB.Group.Nodes.Count && !_gndGroup.Nodes.Contains(lowest))
                {
                    // Merge group A into group B (group A has the least amount of elements)
                    double delta = itemB.Offset - itemA.Offset - offset;
                    foreach (var n in itemA.Group.Nodes)
                    {
                        double newOffset = _dict[n].Offset + delta;
                        _dict[n] = new(itemB.Group, newOffset);
                    }
                    itemB.Group.Nodes.UnionWith(itemA.Group.Nodes);
                }
                else
                {
                    double delta = itemA.Offset - itemB.Offset + offset;
                    foreach (var n in itemB.Group.Nodes)
                    {
                        double newOffset = _dict[n].Offset + delta;
                        _dict[n] = new(itemA.Group, newOffset);
                    }
                    itemA.Group.Nodes.UnionWith(itemB.Group.Nodes);
                }
            }
            else if (hasA && !hasB)
            {
                _found = true;
                _dict[highest] = new(itemA.Group, itemA.Offset + offset);
                itemA.Group.Nodes.Add(highest);
            }
            else if (hasB && !hasA)
            {
                _found = true;
                _dict[lowest] = new(itemB.Group, itemB.Offset - offset);
                itemB.Group.Nodes.Add(lowest);
            }
            else
            {
                _found = true;
                var g = new NodeGroup(lowest, highest);
                _dict.Add(lowest, new(g, 0.0));
                _dict.Add(highest, new(g, offset));
            }
            return true;
        }
    }
}
