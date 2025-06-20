﻿namespace SimpleCircuit.Parser.SvgPathData
{
    /// <summary>
    /// Creates a new <see cref="SvgPathDataLexer"/>
    /// </summary>
    /// <param name="data">The data.</param>
    public class SvgPathDataLexer(string data) : Lexer<TokenType>(data)
    {

        /// <inheritdoc />
        public override bool Check(TokenType flags) => (Type & flags) != 0;

        /// <inheritdoc />
        protected override void ReadToken()
        {
            // White spaces and commas are trivia
            char c = Char;
            while (c == ' ' || c == ',')
            {
                ContinueTrivia();
                c = Char;
            }

            switch (c)
            {
                case '\0':
                    NextType = TokenType.EndOfContent;
                    break;

                case 'A':
                case 'a':
                case 'M':
                case 'm':
                case 'L':
                case 'l':
                case 'h':
                case 'H':
                case 'v':
                case 'V':
                case 'C':
                case 'c':
                case 'S':
                case 's':
                case 'Q':
                case 'q':
                case 'T':
                case 't':
                case 'z':
                case 'Z':
                    // We support everything except for arcs as they don't transform easily
                    NextType = TokenType.Command;
                    ContinueToken();
                    break;

                case char d when char.IsDigit(d) || d == '-' || d == '+' || d == '.':
                    ContinueNumber();
                    NextType = TokenType.Number;
                    break;

                default:
                    NextType = TokenType.Unknown;
                    ContinueToken();
                    break;
            }
        }

        private void ContinueNumber()
        {
            char c = Char;
            if (c == '+' || c == '-')
            {
                ContinueToken();
                c = Char;
            }

            while (char.IsDigit(c))
            {
                ContinueToken();
                c = Char;
            }

            if (c == '.')
            {
                // Fraction
                ContinueToken();
                c = Char;
                while (char.IsDigit(c))
                {
                    ContinueToken();
                    c = Char;
                }
            }

            if (c == 'e' || c == 'E')
            {
                // Exponential notation
                ContinueToken();
                c = Char;
                if (c == '+' || c == '-')
                {
                    ContinueToken();
                    c = Char;
                }
                while (char.IsDigit(c))
                {
                    ContinueToken();
                    c = Char;
                }
            }
        }
    }
}
