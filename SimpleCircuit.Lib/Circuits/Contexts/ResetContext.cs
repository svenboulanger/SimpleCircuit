using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of <see cref="IResetContext"/>.
    /// </summary>
    public class ResetContext : IResetContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Creates a new <see cref="ResetContext"/>.
        /// </summary>
        /// <param name="diagnostics">Diagnostics handler.</param>
        public ResetContext(IDiagnosticHandler diagnostics)
        {
            Diagnostics = diagnostics;
        }
    }
}
