using SimpleCircuit.Diagnostics;
using SpiceSharp.Entities;

namespace SimpleCircuit.Circuits.Contexts;

/// <summary>
/// Describes a context for adding unknowns for solving.
/// </summary>
public interface IRegisterContext
{
    /// <summary>
    /// Gets the diagnostics handler.
    /// </summary>
    public IDiagnosticHandler Diagnostics { get; }

    /// <summary>
    /// Gets the circuit that will be solved.
    /// </summary>
    public IEntityCollection Circuit { get; }

    /// <summary>
    /// Gets the context of the relationships between nodes that will belong to the circuit.
    /// </summary>
    public IPrepareContext Relationships { get; }

    /// <summary>
    /// Gets or sets whether the circuit should be recalculated.
    /// </summary>
    public bool Recalculate { get; set; }

    /// <summary>
    /// Gets a node representative and offset.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>Returns the representative and offset.</returns>
    public RelativeItem GetOffset(string node);
}
