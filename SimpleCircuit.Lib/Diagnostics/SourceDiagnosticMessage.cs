using SimpleCircuit.Parser;

namespace SimpleCircuit.Diagnostics;

/// <summary>
/// Diagnostic message based around a token.
/// </summary>
/// <param name="location">The token.</param>
/// <param name="level">The severity level.</param>
/// <param name="code">The code.</param>
/// <param name="message">The message.</param>
public class SourceDiagnosticMessage(TextLocation location, SeverityLevel level, string code, string message) : DiagnosticMessage(level, code, message)
{
    /// <summary>
    /// Gets the location.
    /// </summary>
    public TextLocation Location { get; } = location;

    /// <inheritdoc />
    public override string ToString()
    {
        string result = $"{Severity}: {Code}: {Message} at line {Location.Line}, column {Location.Column}";
        if (!string.IsNullOrWhiteSpace(Location.Source))
            result += $" in {Location.Source}";
        return result;
    }
}
