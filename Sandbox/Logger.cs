using SimpleCircuit.Diagnostics;
using System;

namespace Sandbox
{
    public class Logger : IDiagnosticHandler
    {
        public int ErrorCount { get; private set; }

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
    }
}
