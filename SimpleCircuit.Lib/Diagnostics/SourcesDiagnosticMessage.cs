using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// A diagnostic messages that can have multiple sources.
    /// </summary>
    public class SourcesDiagnosticMessage : DiagnosticMessage
    {
        /// <summary>
        /// Gets the locations.
        /// </summary>
        public TextLocation[] Locations { get; }

        /// <summary>
        /// Creates a new <see cref="SourcesDiagnosticMessage"/>.
        /// </summary>
        /// <param name="locations">The locations.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public SourcesDiagnosticMessage(IEnumerable<TextLocation> locations, SeverityLevel level, string code, string message)
            : base(level, code, message)
        {
            Locations = locations?.ToArray() ?? Array.Empty<TextLocation>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Severity);
            sb.Append(": ");
            if (Code is not null)
            {
                sb.Append(Code);
                sb.Append(": ");
            }
            sb.Append(Message);

            for (int i = 0; i < Locations.Length; i++)
            {
                if (i == 0)
                    sb.Append(" at ");
                else if (i < Locations.Length - 1)
                    sb.Append(", ");
                else
                    sb.Append(" and ");
                sb.Append("line ");
                sb.Append(Locations[i].Line);
                sb.Append(", ");
                sb.Append("column ");
                sb.Append(Locations[i].Column);
            }
            return sb.ToString();
        }
    }
}
