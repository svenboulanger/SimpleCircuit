using SimpleCircuit.Parser;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleCircuit.Diagnostics;

public static class Helpers
{
    /// <summary>
    /// Gets a diagnostic message for a given error code.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>Returns the diagnostic message.</returns>
    public static IDiagnosticMessage GetDiagnosticMessage(ErrorCodes code, params object[] arguments)
    {
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes is null || attributes.Length == 0)
            return null;
        var info = (DiagnosticAttribute)attributes[0];
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        message = string.Format(message, arguments);
        return new DiagnosticMessage(info.Severity, info.Code, message);
    }

    /// <summary>
    /// Gets a diagnostic message for a given error code.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>Returns the diagnostic message.</returns>
    public static IDiagnosticMessage GetDiagnosticMessage(Token token, ErrorCodes code, params object[] arguments)
    {
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes is null || attributes.Length == 0)
            return null;
        var info = (DiagnosticAttribute)attributes[0];
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        message = string.Format(message, arguments);
        return new SourceDiagnosticMessage(token, info.Severity, info.Code, message);
    }

    /// <summary>
    /// Gets a diagnostic message for a given error code.
    /// </summary>
    /// <param name="location">The token.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>Returns the diagnostic message.</returns>
    public static IDiagnosticMessage GetDiagnosticMessage(TextLocation location, ErrorCodes code, params object[] arguments)
    {
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes is null || attributes.Length == 0)
            return null;
        var info = (DiagnosticAttribute)attributes[0];
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        message = string.Format(message, arguments);
        return new SourceDiagnosticMessage(location, info.Severity, info.Code, message);
    }

    /// <summary>
    /// Gets a diagnostic message for a given error code.
    /// </summary>
    /// <param name="locations">The locations.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>Returns the message, id and severity.</returns>
    public static IDiagnosticMessage GetDiagnosticMessage(IEnumerable<TextLocation> locations, ErrorCodes code, params object[] arguments)
    {
        var attributes = typeof(ErrorCodes).GetField(code.ToString()).GetCustomAttributes(typeof(DiagnosticAttribute), false);
        if (attributes is null || attributes.Length == 0)
            return null;
        var info = (DiagnosticAttribute)attributes[0];
        var message = Properties.Resources.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture);
        message ??= info.Message;
        message = string.Format(message, arguments);
        return new SourcesDiagnosticMessage(locations, info.Severity, info.Code, message);
    }

    /// <summary>
    /// Posts a diagnostic message based on an error code.
    /// </summary>
    /// <param name="handler">The diagnostic message handler.</param>
    /// <param name="code">The error code.</param>
    /// <param name="arguments">The arguments for the error message.</param>
    public static SeverityLevel Post(this IDiagnosticHandler handler, ErrorCodes code, params object[] arguments)
    {
        var message = GetDiagnosticMessage(code, arguments);
        if (message is null)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        handler.Post(message);
        return message.Severity;
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
        var message = GetDiagnosticMessage(token, code, arguments);
        if (message is null)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        handler.Post(message);
        return message.Severity;
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
        var message = GetDiagnosticMessage(location, code, arguments);
        if (message is null)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        handler.Post(message);
        return message.Severity;
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
        var message = GetDiagnosticMessage(code, arguments);
        if (message is null)
        {
            handler.Post(new DiagnosticMessage(SeverityLevel.Error, "?", $"Could not find error code data for '{code}'"));
            return SeverityLevel.Error;
        }
        handler.Post(message);
        return message.Severity;
    }
}
