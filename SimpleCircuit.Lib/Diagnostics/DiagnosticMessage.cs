namespace SimpleCircuit.Diagnostics;

/// <summary>
/// Default implementation for diagnostic message.
/// </summary>
/// <remarks>
/// Creates a new diagnostics message.
/// </remarks>
/// <param name="level">The severity.</param>
/// <param name="code">The code.</param>
/// <param name="message">The message.</param>
public class DiagnosticMessage(SeverityLevel level, string code, string message) : IDiagnosticMessage
{
    /// <inheritdoc />
    public SeverityLevel Severity { get; } = level;

    /// <inheritdoc />
    public string Code { get; } = code;

    /// <inheritdoc />
    public string Message { get; } = message;

    /// <summary>
    /// Converts the message to a string.
    /// </summary>
    /// <returns>The string.</returns>
    public override string ToString()
        => $"{Severity}: {Code}: {Message}";
}
