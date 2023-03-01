using System;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A combination of two nodes representing an X- and Y-coordinate of the same component/block.
    /// </summary>
    /// <remarks>
    /// This allows us to track which X- and Y-coordinates belong to the same graphical group.
    /// </remarks>
    public readonly struct XYNode : IEquatable<XYNode>
    {
        /// <summary>
        /// Gets the X-coordinate node.
        /// </summary>
        public string NodeX { get; }

        /// <summary>
        /// Gets the Y-coordinate node.
        /// </summary>
        public string NodeY { get; }

        /// <summary>
        /// Creates a new linked node.
        /// </summary>
        /// <param name="nodeX">The X-coordinate node.</param>
        /// <param name="nodeY">The Y-coordinate node.</param>
        public XYNode(string nodeX, string nodeY)
        {
            NodeX = nodeX;
            NodeY = nodeY;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = NodeX?.GetHashCode() ?? 0;
            hash *= 21191;
            hash ^= NodeY?.GetHashCode() ?? 0;
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is not XYNode node)
                return false;
            return Equals(node);
        }

        /// <inheritdoc />
        public bool Equals(XYNode other)
        {
            if (NodeX != other.NodeX)
                return false;
            if (NodeY != other.NodeY)
                return false;
            return true;
        }

        /// <inheritdoc />
        public override string ToString() => $"({NodeX}; {NodeY})";
    }
}
