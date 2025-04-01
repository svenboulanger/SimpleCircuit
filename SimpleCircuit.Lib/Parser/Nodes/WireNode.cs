using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A wire.
    /// </summary>
    public record WireNode : SyntaxNode
    {
        /// <summary>
        /// The items of the wire.
        /// </summary>
        public SyntaxNode[] Items { get; }

        /// <summary>
        /// Creates a new <see cref="WireNode"/>.
        /// </summary>
        /// <param name="items">The wire items.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is <c>null</c>.</exception>
        public WireNode(IEnumerable<SyntaxNode> items)
            : base(items?.FirstOrDefault()?.Location ?? default)
        {
            Items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < Items.Length; i++)
                hash = (hash * 1023) ^ Items[i].GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public virtual bool Equals(WireNode other)
        {
            if (Items.Length != other.Items.Length)
                return false;
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != other.Items[i])
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('<');
            if (Items.Length > 0)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (i > 0)
                        sb.Append(' ');
                    sb.Append(Items[i]);
                }
            }
            sb.Append('>');
            return sb.ToString();
        }
    }
}
