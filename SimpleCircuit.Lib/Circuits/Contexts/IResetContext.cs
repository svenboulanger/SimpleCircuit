using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Describes a context for resetting.
    /// </summary>
    public interface IResetContext
    {
        /// <summary>
        /// Gets the diagnostic handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }
    }
}
