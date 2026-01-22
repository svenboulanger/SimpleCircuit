using System;

namespace SimpleCircuit.Diagnostics;

/// <summary>
/// An exception that can wrap diagnostic messages.
/// </summary>
public class SimpleCircuitDiagnosticException : Exception
{
    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public IDiagnosticMessage DiagnosticMessage { get; }

    /// <summary>
    /// Creates a new <see cref="SimpleCircuitDiagnosticException"/>.
    /// </summary>
    public SimpleCircuitDiagnosticException()
    {
    }

    /// <summary>
    /// Creates a new <see cref="SimpleCircuitDiagnosticException"/>.
    /// </summary>
    /// <param name="diagnosticMessage">The diagnostic message.</param>
    public SimpleCircuitDiagnosticException(IDiagnosticMessage diagnosticMessage)
        : base(diagnosticMessage.Message)
    {
        DiagnosticMessage = diagnosticMessage;
    }
}
