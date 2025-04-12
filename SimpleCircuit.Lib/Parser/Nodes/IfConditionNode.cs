using System;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// An if-condition with statements.
    /// </summary>
    public record IfConditionNode : SyntaxNode
    {
        /// <summary>
        /// Gets the if or elif-token.
        /// </summary>
        public Token IfToken {get;}

        /// <summary>
        /// Gets the condition.
        /// </summary>
        public SyntaxNode Condition { get; }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        public ScopedStatementsNode Statements { get; }

        /// <summary>
        /// Creates a new <see cref="IfConditionNode"/>.
        /// </summary>
        /// <param name="ifToken">The if or elif token</param>
        /// <param name="condition">The condition.</param>
        /// <param name="statements">The statements.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="condition"/> is <c>null</c>.</exception>
        public IfConditionNode(Token ifToken, SyntaxNode condition, ScopedStatementsNode statements)
            : base(ifToken.Location)
        {
            IfToken = ifToken;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Statements = statements ?? new([], []);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('.');
            sb.Append(IfToken.Content);
            sb.Append(' ');
            sb.AppendLine(Condition.ToString());
            sb.Append(Statements.ToString());
            return sb.ToString();
        }
    }
}
