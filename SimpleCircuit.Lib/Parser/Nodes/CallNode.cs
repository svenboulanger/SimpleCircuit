using System;

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
        public SyntaxNode Arguments { get; }

        /// <summary>
        /// Creates a new <see cref="CallNode"/>.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="subject"/> is <c>null</c>.</exception>
        public CallNode(SyntaxNode subject, SyntaxNode arguments)
            : base(subject?.Location ?? default)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Arguments = arguments;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Subject}({Arguments})";
    }
}
