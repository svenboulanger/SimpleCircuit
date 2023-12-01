using System;

namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// An attribute for diagnostic messages.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="DiagnosticAttribute"/>.
    /// </remarks>
    /// <param name="level">The severity.</param>
    /// <param name="code">The code.</param>
    /// <param name="message">The message.</param>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class DiagnosticAttribute(SeverityLevel level, string code, string message) : Attribute
    {
        /// <summary>
        /// Gets the severity level of the diagnostic message.
        /// </summary>
        public SeverityLevel Severity { get; } = level;

        /// <summary>
        /// Gets a code for the diagnostic message.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// Gets the default message used if there no localized error message.
        /// </summary>
        public string Message { get; } = message;
    }
}
