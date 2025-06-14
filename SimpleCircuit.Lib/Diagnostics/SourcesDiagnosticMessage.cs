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
    /// <param name="locations">The locations.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="code">The code.</param>
    /// <param name="message">The message.</param>
    public class SourcesDiagnosticMessage(IEnumerable<TextLocation> locations, SeverityLevel level, string code, string message) : DiagnosticMessage(level, code, message)
    {
        /// <summary>
        /// Gets the locations.
        /// </summary>
        public TextLocation[] Locations { get; } = locations?.ToArray() ?? [];

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new();

            // Start with the severity
            sb.Append(Severity);
            sb.Append(": ");

            // If the code is defined, also pass it
            if (Code is not null)
            {
                sb.Append(Code);
                sb.Append(": ");
            }

            // Add the message
            sb.Append(Message);

            // Now we will append the list of text locations
            var srcs = new Dictionary<string, List<string>>();
            foreach (var location in Locations)
            {
                string src = string.IsNullOrWhiteSpace(location.Source) ? string.Empty : location.Source;
                if (!srcs.TryGetValue(src, out var list))
                {
                    list = [];
                    srcs.Add(src, list);
                }
                list.Add($"line {location.Line}, column {location.Column}");
            }

            // Show the sources
            int index = 0;
            foreach (var pair in srcs.OrderBy(p => p.Key))
            {
                if (index == 0)
                    sb.Append(" at ");
                else if (index < srcs.Count - 1)
                    sb.Append("; at ");
                else
                    sb.Append(" and at ");
                sb.Append(string.Join(", ", pair.Value));
                if (pair.Key.Length > 0)
                {
                    sb.Append(" in ");
                    sb.Append(pair.Key);
                }
                index++;
            }
            return sb.ToString();
        }
    }
}
