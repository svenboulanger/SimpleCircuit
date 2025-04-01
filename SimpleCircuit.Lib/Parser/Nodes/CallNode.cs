using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A call.
    /// </summary>
    public record CallNode : SyntaxNode, IEquatable<CallNode>
    {
        /// <summary>
        /// The subject that is called.
        /// </summary>
        public SyntaxNode Subject { get; }

        /// <summary>
        /// The arguments of the call.
        /// </summary>
        public SyntaxNode[] Arguments { get; }

        /// <summary>
        /// Creates a new <see cref="CallNode"/>.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="subject"/> is <c>null</c>.</exception>
        public CallNode(SyntaxNode subject, IEnumerable<SyntaxNode> arguments)
            : base(subject?.Location ?? default)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Arguments = arguments?.ToArray() ?? Array.Empty<SyntaxNode>();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = Subject.GetHashCode();
            for (int i = 0; i < Arguments.Length; i++)
                hash = (hash * 1023) ^ Arguments[i].GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public virtual bool Equals(CallNode call)
        {
            if (!Subject.Equals(call.Subject))
                return false;
            if (Arguments.Length != call.Arguments.Length)
                return false;
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (!Arguments[i].Equals(call.Arguments[i]))
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Subject.ToString());
            sb.Append('(');
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(Arguments[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
