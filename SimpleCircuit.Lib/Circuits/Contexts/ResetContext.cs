using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of <see cref="IResetContext"/>.
    /// </summary>
    public class ResetContext : IResetContext
    {
        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public IElementFormatter Formatter { get; }

        /// <summary>
        /// Creates a new <see cref="ResetContext"/>.
        /// </summary>
        /// <param name="diagnostics">Diagnostics handler.</param>
        /// <param name="formatter">Element formatter.</param>
        public ResetContext(IDiagnosticHandler diagnostics, IElementFormatter formatter)
        {
            Diagnostics = diagnostics;
            Formatter = formatter ?? new ElementFormatter();
        }
    }
}
