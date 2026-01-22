namespace SimpleCircuit.Diagnostics;

/// <summary>
/// Describes a diagnostic message.
/// </summary>
public interface IDiagnosticMessage
{
    /// <summary>
    /// Gets the severity level of the message.
    /// </summary>
    public SeverityLevel Severity { get; }

    /// <summary>
    /// Gets the code (identifier) of the message.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the message itself.
    /// </summary>
    public string Message { get; }
}
