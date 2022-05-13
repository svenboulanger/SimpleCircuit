using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit
{
    /// <summary>
    /// A diagnostic logger for a job.
    /// </summary>
    public class JobDiagnosticLogger : IDiagnosticHandler
    {
        /// <summary>
        /// Gets the messages received by the logger.
        /// </summary>
        public List<IDiagnosticMessage> Messages { get; } = new List<IDiagnosticMessage>();

        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
            => Messages.Add(message);
    }
}
