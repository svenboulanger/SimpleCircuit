using System;

namespace SimpleCircuit
{
    /// <summary>
    /// Event arguments for warning messages.
    /// </summary>
    public class WarningEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the warning message.
        /// </summary>
        /// <value>
        /// The warning message.
        /// </value>
        public string Message { get; }

        /// <summary>
        /// Creates a new warning event argument.
        /// </summary>
        /// <param name="message">The message.</param>
        public WarningEventArgs(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
