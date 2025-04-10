using System;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A symbol definition node.
    /// </summary>
    public record SymbolDefinitionNode : SyntaxNode
    {
        /// <summary>
        /// Gets symbol key.
        /// </summary>
        public Token Key { get; }

        /// <summary>
        /// Gets the XML node.
        /// </summary>
        public XmlNode Xml { get; }

        /// <summary>
        /// Creates a new <see cref="SymbolDefinitionNode"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="node">The node.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SymbolDefinitionNode(Token key, XmlNode node)
            : base(key.Location)
        {
            Key = key;
            Xml = node ?? throw new ArgumentNullException(nameof(node));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(".symbol ");
            sb.AppendLine(Key.Content.ToString());
            sb.AppendLine(Xml.OuterXml);
            sb.AppendLine(".endsymbol");
            return sb.ToString();
        }
    }
}
