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
        /// <summary>
        /// The ground nodes.
        /// </summary>
        public static string[] _ground = new[] { "0", "gnd", "gnd!" };

        private readonly struct NodeGroupItem
        {
            public NodeGroup Group { get; }
            public double Offset { get; }
            public NodeGroupItem(NodeGroup group, double offset)
            {
                Group = group;
                Offset = offset;
            }
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
        private readonly Dictionary<string, ExtremeTracker> _bounds = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets all the representatives in the grouper.
        /// </summary>
        public IEnumerable<string> Representatives
        {
            get
            {
                if (_bounds.Count > 0)
                {
                    foreach (var key in _bounds.Keys)
                        yield return key;
                }
                else
                {
                    HashSet<string> done = new(StringComparer.OrdinalIgnoreCase);
                    foreach (var item in _dict.Values)
                    {
                        if (done.Add(item.Group.Representative))
                            yield return item.Group.Representative;
                    }
                }
            }
        }

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
                _dict[highest] = new(itemA.Group, itemA.Offset + offset);
                itemA.Group.Nodes.Add(highest);
            }
            else if (hasB && !hasA)
            {
                _dict[lowest] = new(itemB.Group, itemB.Offset - offset);
                itemB.Group.Nodes.Add(lowest);
            }
            else
            {
                var g = new NodeGroup(lowest, highest);
                _dict.Add(lowest, new(g, 0.0));
                _dict.Add(highest, new(g, offset));
            }
            return true;
        }

        /// <summary>
        /// Computes the bounds for all representatives based on all the 
        /// offsets having been stored inside.
        /// </summary>
        public void ComputeBounds()
        {
            _bounds.Clear();
            foreach (var pair in _dict)
            {
                if (!_bounds.TryGetValue(pair.Value.Group.Representative, out var bounds))
                {
                    bounds = new();
                    _bounds.Add(pair.Value.Group.Representative, bounds);
                }
                bounds.Expand(pair.Value.Offset);
            }
        }

        /// <summary>
        /// Gets the bounds for a representative node.
        /// </summary>
        /// <param name="representative">The representative.</param>
        /// <returns>The extremes associated to the representative.</returns>
        public ExtremeTracker GetBounds(string representative)
        {
            if (!_bounds.TryGetValue(representative, out var extremes))
                return new();
            return extremes;
        }
    }
}
