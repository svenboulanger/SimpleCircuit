using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A subcircuit definition.
    /// </summary>
    public record SubcircuitDefinitionNode : SyntaxNode
    {
        /// <summary>
        /// Gets the SUBCKT keyword.
        /// </summary>
        public Token Subckt { get; }

        /// <summary>
        /// Gets the subcircuit name.
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// Gets the pins.
        /// </summary>
        public SyntaxNode[] Pins { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public SyntaxNode[] Properties { get; }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        public ScopedStatementsNode Statements { get; }

        /// <summary>
        /// Creates a new <see cref="SubcircuitDefinitionNode"/>.
        /// </summary>
        /// <param name="subckt">The subcircuit keyword.</param>
        /// <param name="name">The subcircuit name.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="statements">The statements.</param>
        public SubcircuitDefinitionNode(Token subckt, Token name, IEnumerable<SyntaxNode> pins, IEnumerable<SyntaxNode> properties, ScopedStatementsNode statements)
            : base(subckt.Location)
        {
            Subckt = subckt;
            Name = name;
            Pins = pins?.ToArray() ?? [];
            Properties = properties?.ToArray() ?? [];
            Statements = statements ?? ScopedStatementsNode.Empty;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Subckt.Content);
            sb.Append(' ');
            sb.Append(Name.Content);

            // The pins
            foreach (var pin in Pins)
            {
                sb.Append(' ');
                sb.Append(pin.ToString());
            }

            // The properties
            foreach (var property in Properties)
            {
                sb.Append(' ');
                sb.Append(property.ToString());
            }
            sb.AppendLine();

            // The statements
            sb.AppendLine(Statements.ToString());

            sb.Append(".endsubckt");
            return sb.ToString();
        }
    }
}
