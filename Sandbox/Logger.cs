using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox
{
    public class Logger : IDiagnosticHandler
    {
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info:
                    Console.WriteLine(message.Message);
                    break;

                case SeverityLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: {message.Code}: {message.Message}");
                    Console.ResetColor();
                    break;

                case SeverityLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {message.Code}: {message.Message}");
                    Console.ResetColor();
                    break;
            }
        }
    }
}
