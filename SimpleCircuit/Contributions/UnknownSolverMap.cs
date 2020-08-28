using SimpleCircuit.Components;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{

    /// <summary>
    /// A map for unknowns in a solver.
    /// </summary>
    public class UnknownSolverMap
    {
        private struct Node
        {
            public readonly IContributor Owner;
            public readonly UnknownTypes Type;
            public Node(IContributor owner, UnknownTypes type)
            {
                Owner = owner;
                Type = type;
            }
            public override int GetHashCode()
            {
                return Owner.GetHashCode() ^ Type.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is Node node)
                {
                    if (!ReferenceEquals(Owner, node.Owner))
                        return false;
                    if (Type != node.Type)
                        return false;
                    return true;
                }
                return false;
            }
        }
        private readonly Dictionary<int, Node> _map = new Dictionary<int, Node>();
        private readonly Dictionary<Node, int> _invMap = new Dictionary<Node, int>();

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        /// <value>
        /// The number of variables.
        /// </value>
        public int Count => _map.Count;

        /// <summary>
        /// Gets the variable type.
        /// </summary>
        /// <value>
        /// The <see cref="UnknownTypes"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The variable type.</returns>
        public UnknownTypes this[int index] => _map[index].Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownSolverMap"/> class.
        /// </summary>
        public UnknownSolverMap()
        {
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _map.Clear();
            _invMap.Clear();
        }

        /// <summary>
        /// Creates the unknown.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The index/column that this variable will use in the solver.
        /// </returns>
        public int GetUnknown(IContributor owner, UnknownTypes type)
        {
            var node = new Node(owner, type);
            if (!_invMap.TryGetValue(node, out var index))
            {
                index = _map.Count + 1;
                _map.Add(index, node);
                _invMap.Add(node, index);
                return index;
            }
            return index;
        }

        /// <summary>
        /// Gets the owner of the unknown with the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The contributor that owns this unknown.</returns>
        public IContributor GetOwner(int index) => _map[index].Owner;

        /// <summary>
        /// Tries the index of the get.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="type">The type.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool TryGetIndex(IContributor owner, UnknownTypes type, out int index)
            => _invMap.TryGetValue(new Node(owner, type), out index);
    }
}
