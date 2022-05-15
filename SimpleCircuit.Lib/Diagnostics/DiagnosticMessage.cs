namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// Default implementation for diagnostic message.
    /// </summary>
    public class DiagnosticMessage : IDiagnosticMessage
    {
        /// <inheritdoc />
        public SeverityLevel Severity { get; }

        /// <inheritdoc />
        public string Code { get; }

        /// <inheritdoc />
        public string Message { get; }

        /// <summary>
        /// Creates a new diagnostics message.
        /// </summary>
        /// <param name="level">The severity.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public DiagnosticMessage(SeverityLevel level, string code, string message)
        {
            Severity = level;
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Converts the message to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => $"{Severity}: {Code}: {Message}";
    }
}
