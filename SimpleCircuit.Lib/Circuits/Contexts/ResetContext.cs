using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of <see cref="IResetContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="ResetContext"/>.
    /// </remarks>
    /// <param name="diagnostics">Diagnostics handler.</param>
    public class ResetContext(IDiagnosticHandler diagnostics) : IResetContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;
    }
}
