using SimpleCircuit.Diagnostics;

namespace SimpleCircuitOnline.Shared
{
    /// <summary>
    /// A diagnostic message for when the view mode is on.
    /// </summary>
    public class ViewModeDiagnosticMessage : DiagnosticMessage
    {
        /// <summary>
        /// Creates a new <see cref="ViewModeDiagnosticMessage"/>.
        /// </summary>
        public ViewModeDiagnosticMessage()
            : base(SeverityLevel.Info, null, "This work was not saved to local memory")
        {

        }
    }
}
