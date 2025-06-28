using SimpleCircuit.Diagnostics;
using System;

namespace Sandbox
{
    /// <summary>
    /// A logger that passes diagnostic messages to the console.
    /// </summary>
    public class Logger : IDiagnosticHandler
    {
        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
        {
            if (message == null)
                return;
            Console.ForegroundColor = message.Severity switch
            {
                SeverityLevel.Warning => ConsoleColor.Yellow,
                SeverityLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.WriteLine(message);
            Console.ResetColor();

            if (message.Severity == SeverityLevel.Error)
                ErrorCount++;
        }

        /// <summary>
        /// Reset the error count.
        /// </summary>
        public void Reset()
        {
            ErrorCount = 0;
        }
    }
}
