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
        public List<IDiagnosticMessage> Messages { get; } = new();

        /// <summary>
        /// Gets the number of errors tracked by the logger.
        /// </summary>
        public int Errors { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Logger"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info: break;
                case SeverityLevel.Warning:
                    Messages.Add(message);
                    break;

                case SeverityLevel.Error:
                    Messages.Add(message);
                    Errors++;
                    break;
            }
        }

        /// <summary>
        /// Clears any messages in the logger.
        /// </summary>
        public void Clear()
        {
            Messages.Clear();
            Errors = 0;
        }
    }
}
