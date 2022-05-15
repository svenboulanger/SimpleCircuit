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
        /// The informational messages.
        /// </summary>
        public StringWriter Info { get; } = new();

        /// <summary>
        /// The warning messages.
        /// </summary>
        public StringWriter Warning { get; } = new();

        /// <summary>
        /// The error messages.
        /// </summary>
        public StringWriter Error { get; } = new();

        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        public int ErrorCount { get; private set; }

        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info: Info.WriteLine(message); break;
                case SeverityLevel.Warning: Warning.WriteLine(message); break;
                case SeverityLevel.Error: Error.WriteLine(message); ErrorCount++; break;
            }
        }
    }
}
