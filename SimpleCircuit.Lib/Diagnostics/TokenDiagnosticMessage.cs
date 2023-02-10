using SimpleCircuit.Parser;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// Diagnostic message based around a token.
    /// </summary>
    public class TokenDiagnosticMessage : DiagnosticMessage
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        public Token Token { get; }

        /// <summary>
        /// Creates a new <see cref="TokenDiagnosticMessage"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public TokenDiagnosticMessage(Token token, SeverityLevel level, string code, string message)
            : base(level, code, message)
        {
            Token = token;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string result = $"{Severity}: {Code}: {Message} at line {Token.Location.Line}, column {Token.Location.Column}";
            if (!string.IsNullOrWhiteSpace(Token.Source))
                result += $" {Token.Source}";
            return result;
        }
    }
}
