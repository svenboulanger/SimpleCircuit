using SimpleCircuit.Parser;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// Diagnostic message based around a token.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SourceDiagnosticMessage"/>.
    /// </remarks>
    public class SourceDiagnosticMessage : DiagnosticMessage
    {
        /// <summary>
        /// Gets the location.
        /// </summary>
        public TextLocation Location { get; }

        /// <summary>
        /// Creates a new <see cref="SourceDiagnosticMessage"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public SourceDiagnosticMessage(Token token, SeverityLevel level, string code, string message)
            : base(level, code, message)
        {
            Location = token.Location;
        }

        /// <summary>
        /// Creates a new <see cref="SourceDiagnosticMessage"/>.
        /// </summary>
        /// <param name="location">The token.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public SourceDiagnosticMessage(TextLocation location, SeverityLevel level, string code, string message)
            : base(level, code, message)
        {
            Location = location;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string result = $"{Severity}: {Code}: {Message} at line {Location.Line}, column {Location.Column}";
            if (!string.IsNullOrWhiteSpace(Location.Source))
                result += $" in {Location.Source}";
            return result;
        }
    }
}
