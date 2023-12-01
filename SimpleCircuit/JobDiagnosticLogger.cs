using SimpleCircuit.Diagnostics;

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
        public List<IDiagnosticMessage> Messages { get; } = [];

        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
            => Messages.Add(message);
    }
}
