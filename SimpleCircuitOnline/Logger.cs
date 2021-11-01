using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleCircuit.Diagnostics;

namespace SimpleCircuitOnline
{
    public class Logger : IDiagnosticHandler
    {
        public StringWriter Info { get; } = new();

        public StringWriter Warning { get; } = new();

        public StringWriter Error { get; } = new();

        public void Post(IDiagnosticMessage message)
        {
            switch (message.Severity)
            {
                case SeverityLevel.Info: Info.WriteLine($"{message.Message}"); break;
                case SeverityLevel.Warning: Warning.WriteLine($"Warning: {message.Code}: {message.Message}"); break;
                case SeverityLevel.Error: Error.WriteLine($"Error: {message.Code}: {message.Message}"); break;
            }
        }
    }
}
