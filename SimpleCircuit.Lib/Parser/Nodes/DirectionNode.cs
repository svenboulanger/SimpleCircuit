using System;
using System.Text;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// A direction.
    /// </summary>
    public record DirectionNode : SyntaxNode
    {
        /// <summary>
        /// An arrow pointing left.
        /// </summary>
        public const string LeftArrow = "\u2190";

        /// <summary>
        /// An arrow pointing up.
        /// </summary>
        public const string UpArrow = "\u2191";

        /// <summary>
        /// An arrow pointing right.
        /// </summary>
        public const string RightArrow = "\u2192";

        /// <summary>
        /// An arrow pointing down.
        /// </summary>
        public const string DownArrow = "\u2193";

        /// <summary>
        /// An arrow pointing up-left.
        /// </summary>
        public const string UpLeftArrow = "\u2196";

        /// <summary>
        /// An arrow pointing up-right.
        /// </summary>
        public const string UpRightArrow = "\u2197";

        /// <summary>
        /// An arrow pointing down-right.
        /// </summary>
        public const string DownRightArrow = "\u2198";

        /// <summary>
        /// An arrow pointing down-left.
        /// </summary>
        public const string DownLeftArrow = "\u2199";

        /// <summary>
        /// Gets the value.
        /// </summary>
        public ReadOnlyMemory<char> Value { get; }

        /// <summary>
        /// Gets or sets the angle of the direction.
        /// </summary>
        public SyntaxNode Angle { get; }

        /// <summary>
        /// Gets or sets the distance of the direction.
        /// </summary>
        public SyntaxNode Distance { get; }

        /// <summary>
        /// Creates a new <see cref="DirectionNode"/>.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="distance">the distance.</param>
        public DirectionNode(Token direction, SyntaxNode angle, SyntaxNode distance)
            : base(direction.Location)
        {
            Value = direction.Content;
            Angle = angle;
            Distance = distance;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append("dir(");
            sb.Append(Value);
            if (Angle is not null)
            {
                sb.Append(' ');
                sb.Append(Angle);
            }
            if (Distance is not null)
            {
                sb.Append(' ');
                sb.Append(Distance);
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
