using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuitOnline
{
    /// <summary>
    /// A logger for diagnostic messages.
    /// </summary>
    public class Logger : IDiagnosticHandler
    {
        /// <summary>
        /// Gets the error messages.
        /// </summary>
        public List<string> Errors { get; } = new();

        /// <summary>
        /// Gets the warning messages.
        /// </summary>
        public List<string> Warnings { get; } = new();

        /// <summary>
        /// Gets the informational messages.
        /// </summary>
        public List<string> Info { get; } = new();

        /// <summary>
        /// Creates a new <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info: Info.Add(message.ToString()); break;
                case SeverityLevel.Warning: Warnings.Add(message.ToString()); break;
                case SeverityLevel.Error: Errors.Add(message.ToString()); break;
            }
        }

        /// <summary>
        /// Clears any messages in the logger.
        /// </summary>
        public void Clear()
        {
            Errors.Clear();
            Warnings.Clear();
            Info.Clear();
        }
    }
}
