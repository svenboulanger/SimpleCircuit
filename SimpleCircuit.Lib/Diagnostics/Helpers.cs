using SimpleCircuit.Parser;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleCircuit.Diagnostics;

public static class Helpers
{
    /// <summary>
    /// Posts a diagnostic message based on an error code.
    /// </summary>
    /// <param name="handler">The diagnostic message handler.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments for the error message.</param>
    public static SeverityLevel Post(this IDiagnosticHandler handler, ErrorCodes code, params object[] arguments)
    {
        // Get the severity and code from the attribute
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes == null || attributes.Length == 0)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        var info = (DiagnosticAttribute)attributes[0];

        // Search for the message in the resource
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        handler.Post(new DiagnosticMessage(info.Severity, info.Code, string.Format(message, arguments)));
        return info.Severity;
    }

    /// <summary>
    /// Posts a diagnostic message based on an error code and token.
    /// </summary>
    /// <param name="handler">The diagnostic message handler.</param>
    /// <param name="token">The token.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments for the error message.</param>
    public static SeverityLevel Post(this IDiagnosticHandler handler, Token token, ErrorCodes code, params object[] arguments)
    {
        // Get the severity and code from the attribute
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes == null || attributes.Length == 0)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        var info = (DiagnosticAttribute)attributes[0];

        // Search for the message in the resource
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        handler.Post(new SourceDiagnosticMessage(token, info.Severity, info.Code, string.Format(message, arguments)));
        return info.Severity;
    }

    /// <summary>
    /// Posts a diagnostic message based on an error code and text location.
    /// </summary>
    /// <param name="handler">The diagnostic message handler.</param>
    /// <param name="location">The source location.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments for the error message.</param>
    /// <returns>The diagnostic severity.</returns>
    public static SeverityLevel Post(this IDiagnosticHandler handler, TextLocation location, ErrorCodes code, params object[] arguments)
    {
        // Get the severity and code from the attribute
        object[] attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes == null || attributes.Length == 0)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        var info = (DiagnosticAttribute)attributes[0];

        // Search for the message in the resource
        string message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        handler.Post(new SourceDiagnosticMessage(location, info.Severity, info.Code, string.Format(message, arguments)));
        return info.Severity;
    }

    /// <summary>
    /// Posts a diagnostic messages based on an error code and text locations.
    /// </summary>
    /// <param name="handler">The diagnostic message handler.</param>
    /// <param name="locations">The source location.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments for the error message.</param>
    /// <returns>The diagnostic severity.</returns>
    public static SeverityLevel Post(this IDiagnosticHandler handler, IEnumerable<TextLocation> locations, ErrorCodes code, params object[] arguments)
    {
        // Get the severity and code from the attribute
        object[] attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes == null || attributes.Length == 0)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        var info = (DiagnosticAttribute)attributes[0];

        // Search for the message in the resource
        string message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        handler.Post(new SourcesDiagnosticMessage(locations, info.Severity, info.Code, string.Format(message, arguments)));
        return info.Severity;
    }
}
