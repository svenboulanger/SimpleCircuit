using System;

namespace SimpleCircuit.Parser.Markers
{
    /// <summary>
    /// A lexer for markers.
    /// </summary>
    /// <remarks>
    /// Creates a new marker lexer.
    /// </remarks>
    /// <param name="content">The content.</param>
    public class MarkerLexer(string content) : Lexer<TokenType>(content.AsMemory())
    {

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            char c;
            while ((c = Char) == ' ' || c == '+')
                ContinueTrivia();

            if (c == '\0')
                Type = TokenType.EndOfContent;
            else
            {
                Type = TokenType.Marker;
                while ((c = Char) != '\0' && c != ' ' && c != '+')
                    ContinueToken();
            }
        }
    }
}
