using System;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// An attribute for diagnostic messages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DiagnosticAttribute : Attribute
    {
        /// <summary>
        /// Gets the severity level of the diagnostic message.
        /// </summary>
        public SeverityLevel Severity { get; }

        /// <summary>
        /// Gets a code for the diagnostic message.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the default message used if there no localized error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a new <see cref="DiagnosticAttribute"/>.
        /// </summary>
        /// <param name="level">The severity.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        public DiagnosticAttribute(SeverityLevel level, string code, string message)
        {
            Severity = level;
            Code = code;
            Message = message;
        }
    }
}
