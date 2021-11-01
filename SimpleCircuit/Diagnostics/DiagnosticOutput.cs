using System;
using System.IO;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// A default implementation of a diagnostic handler that refers diagnostic messages
    /// to text writers.
    /// </summary>
    public class DiagnosticOutput : IDiagnosticHandler
    {
        private readonly TextWriter _info, _warning, _error;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticOutput"/> class.
        /// </summary>
        /// <param name="info">The informational message text writer.</param>
        /// <param name="warning">The warning message text writer.</param>
        /// <param name="error">The error message text writer.</param>
        public DiagnosticOutput(TextWriter info, TextWriter warning, TextWriter error)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info));
            _warning = warning ?? throw new ArgumentNullException(nameof(warning));
            _error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info:
                    _info.WriteLine($"Info: {message.Code}: {message.Message}");
                    break;

                case SeverityLevel.Warning:
                    _warning.WriteLine($"Warning: {message.Code}: {message.Message}");
                    break;

                case SeverityLevel.Error:
                    _error.WriteLine($"Error: {message.Code}: {message.Message}");
                    break;
            }
        }
    }
}
