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
    public class MarkerLexer(string content) : Lexer<TokenType>(content)
    {

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (NextType & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            char c;
            while ((c = Char) == ' ' || c == '+')
                ContinueTrivia();

            if (c == '\0')
                NextType = TokenType.EndOfContent;
            else
            {
                NextType = TokenType.Marker;
                while ((c = Char) != '\0' && c != ' ' && c != '+')
                    ContinueToken();
            }
        }
    }
}
