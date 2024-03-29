﻿using SimpleCircuit.Diagnostics;

namespace SimpleCircuit
{
    /// <summary>
    /// An <see cref="IDiagnosticHandler"/> that logs to the console.
    /// </summary>
    public class ConsoleDiagnosticLogger : IDiagnosticHandler
    {
        /// <summary>
        /// Gets the number of errors encountered.
        /// </summary>
        public int Errors { get; private set; }

        /// <summary>
        /// Gets the number of warnings encountered.
        /// </summary>
        public int Warnings { get; private set; }

        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info:
                    Console.WriteLine(message);
                    break;

                case SeverityLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(message);
                    Console.ResetColor();
                    break;

                case SeverityLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(message);
                    Console.ResetColor();
                    break;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"Logger - {Errors} errors and {Warnings} warnings";
    }
}
