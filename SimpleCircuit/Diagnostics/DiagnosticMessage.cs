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

        public DiagnosticMessage(SeverityLevel level, string code, string message)
        {
            Severity = level;
            Code = code;
            Message = message;
        }
    }
}
