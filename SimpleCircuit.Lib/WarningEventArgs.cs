using System;

namespace SimpleCircuit
{
    /// <summary>
    /// Event arguments for warning messages.
    /// </summary>
    /// <remarks>
    /// Creates a new warning event argument.
    /// </remarks>
    /// <param name="message">The message.</param>
    public class WarningEventArgs(string message) : EventArgs
    {
        /// <summary>
        /// Gets the warning message.
        /// </summary>
        /// <value>
        /// The warning message.
        /// </value>
        public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
    }
}
