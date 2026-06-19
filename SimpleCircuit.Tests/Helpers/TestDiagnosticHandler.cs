using System.Collections.Generic;
using System.Linq;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Tests.Helpers;

/// <summary>
/// An <see cref="IDiagnosticHandler"/> that records every posted message so tests
/// can assert on "no errors" or on a specific error code.
/// </summary>
public sealed class TestDiagnosticHandler : IDiagnosticHandler
{
    /// <summary>
    /// Gets all messages that were posted, in order.
    /// </summary>
    public List<IDiagnosticMessage> Messages { get; } = [];

    /// <summary>
    /// Gets whether any error-severity message was posted.
    /// </summary>
    public bool HasErrors => Messages.Any(m => m.Severity == SeverityLevel.Error);

    /// <summary>
    /// Gets all error codes that were posted.
    /// </summary>
    public IEnumerable<string> Codes => Messages.Select(m => m.Code);

    /// <inheritdoc />
    public void Post(IDiagnosticMessage message) => Messages.Add(message);
}
