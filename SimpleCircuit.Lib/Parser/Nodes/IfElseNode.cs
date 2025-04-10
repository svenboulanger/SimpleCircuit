using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// An if-else node.
    /// </summary>
    public record IfElseNode : SyntaxNode
    {
        /// <summary>
        /// Gets the conditions.
        /// </summary>
        public SyntaxNode[] Conditions { get; }

        /// <summary>
        /// Gets the statements for each condition.
        /// </summary>
        public SyntaxNode[][] IfTrue { get; }

        /// <summary>
        /// Gets the statements if no condition matched.
        /// </summary>
        public SyntaxNode[] Else { get; }

        /// <summary>
        /// Creates a new <see cref="IfElseNode"/>.
        /// </summary>
        /// <param name="ifLocation">The location of the .if-word.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="ifTrueStatements">The statements for each condition.</param>
        /// <param name="elseStatements">The statements if no condition matched.</param>
        public IfElseNode(TextLocation ifLocation, IEnumerable<SyntaxNode> conditions, IEnumerable<IEnumerable<SyntaxNode>> ifTrueStatements, IEnumerable<SyntaxNode> elseStatements)
            : base(ifLocation)
        {
            Conditions = conditions?.ToArray() ?? throw new ArgumentNullException(nameof(conditions));
            var args = ifTrueStatements?.ToArray() ?? throw new ArgumentNullException(nameof(ifTrueStatements));
            if (args.Length != Conditions.Length)
                throw new ArgumentException($"Expected the same number of true statements as there are conditions");
            IfTrue = new SyntaxNode[args.Length][];
            for (int i = 0; i < args.Length; i++)
                IfTrue[i] = args[i]?.ToArray() ?? throw new ArgumentNullException(nameof(ifTrueStatements));
            Else = elseStatements?.ToArray(); // Can be left empty
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < Conditions.Length; i++)
            {
                if (i == 0)
                    sb.Append(".if ");
                else
                    sb.Append(".elif");
                sb.AppendLine(Conditions[i].ToString());
                foreach (var statement in IfTrue[i])
                    sb.AppendLine(statement.ToString());
            }
            if (Else is not null)
            {
                sb.AppendLine(".else");
                foreach (var statement in Else)
                    sb.AppendLine(statement.ToString());
            }
            sb.AppendLine(".endif");
            return sb.ToString();
        }
    }
}
