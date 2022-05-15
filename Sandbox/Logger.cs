using SimpleCircuit.Diagnostics;
using System;

namespace Sandbox
{
    public class Logger : IDiagnosticHandler
    {
        public void Post(IDiagnosticMessage message)
        {
            Console.ForegroundColor = message.Severity switch
            {
                SeverityLevel.Warning => ConsoleColor.Yellow,
                SeverityLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
