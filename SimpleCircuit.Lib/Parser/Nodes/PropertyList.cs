using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A list of properties.
    /// </summary>
    public record PropertyList : SyntaxNode, IEquatable<PropertyList>
    {
        /// <summary>
        /// Gets the subject that should receive the properties.
        /// </summary>
        public SyntaxNode Subject { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public SyntaxNode[] Properties { get; }

        /// <summary>
        /// Creates a new <see cref="PropertyList"/>.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="properties">The properties.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="subject"/> is <c>null</c>.</exception>
        public PropertyList(SyntaxNode subject, IEnumerable<SyntaxNode> properties)
            : base(subject?.Location ?? default)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Properties = properties?.ToArray() ?? Array.Empty<SyntaxNode>();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = Subject.GetHashCode();
            for (int i = 0; i < Properties.Length; i++)
                hash = (hash * 1023) ^ Properties[i].GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public virtual bool Equals(PropertyList other)
        {
            if (!Subject.Equals(other.Subject))
                return false;
            if (Properties.Length != other.Properties.Length)
                return false;
            for (int i = 0; i < Properties.Length; i++)
            {
                if (Properties[i] != other.Properties[i])
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Subject);
            if (Properties.Length > 0)
            {
                sb.Append('(');
                for (int i = 0; i < Properties.Length; i++)
                {
                    if (i > 0)
                        sb.Append(' ');
                    sb.Append(Properties[i]);
                }
                sb.Append(')');
            }
            return sb.ToString();
        }
    }
}
