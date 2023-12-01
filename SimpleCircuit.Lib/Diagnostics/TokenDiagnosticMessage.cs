using SimpleCircuit.Parser;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// Diagnostic message based around a token.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="TokenDiagnosticMessage"/>.
    /// </remarks>
    /// <param name="token">The token.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="code">The code.</param>
    /// <param name="message">The message.</param>
    public class TokenDiagnosticMessage(Token token, SeverityLevel level, string code, string message) : DiagnosticMessage(level, code, message)
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        public Token Token { get; } = token;

        /// <inheritdoc />
        public override string ToString()
        {
            string result = $"{Severity}: {Code}: {Message} at line {Token.Location.Line}, column {Token.Location.Column}";
            if (!string.IsNullOrWhiteSpace(Token.Source))
                result += $" in {Token.Source}";
            return result;
        }
    }
}
