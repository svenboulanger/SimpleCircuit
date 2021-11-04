namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// Describes a handler for diagnostic messages.
    /// </summary>
    public interface IDiagnosticHandler
    {
        /// <summary>
        /// Posts a new diagnostic message.
        /// </summary>
        /// <param name="message">The diagnostic message.</param>
        public void Post(IDiagnosticMessage message);
    }
}
