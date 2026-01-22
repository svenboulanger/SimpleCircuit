using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Parser;

/// <summary>
/// A parsing context.
/// </summary>
public class ParsingContext
{
    /// <summary>
    /// A flag that determines whether subcircuit or symbol definitions are allowed.
    /// They change the factory dictionary, so typically you don't want them nested.
    /// </summary>
    public bool AllowFactoryExtension { get; set; } = true;

    /// <summary>
    /// Gets or sets the diagnostics handler.
    /// </summary>
    public IDiagnosticHandler Diagnostics { get; set; }

    /// <summary>
    /// Gets the referenced variables in the current scope.
    /// </summary>
    public HashSet<string> ReferencedVariables { get; } = [];

    /// <summary>
    /// Resets the parsing context.
    /// </summary>
    public void Reset()
    {
        AllowFactoryExtension = true;
        ReferencedVariables.Clear();
    }
}
