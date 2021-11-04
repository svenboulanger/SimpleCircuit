using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit
{
    public class Logger : IDiagnosticHandler
    {
        /// <inheritdoc />
        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info:
                    Console.WriteLine(Properties.Resources.Info, message.Code, message.Message);
                    break;
                case SeverityLevel.Warning:
                    Console.WriteLine(Properties.Resources.Warning, message.Code, message.Message);
                    break;
                case SeverityLevel.Error:
                    Console.WriteLine(Properties.Resources.Error, message.Code, message.Message);
                    break;
            }
        }
    }
}
