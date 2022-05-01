using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace SimpleCircuit
{
    /// <summary>
    /// General helper methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// The culture used for parsing.
        /// </summary>
        public readonly static CultureInfo Culture = CultureInfo.InvariantCulture;

        public class ComponentDescription
        {
            public string Key { get; }
            public string Name { get; }
            public string Category { get; }
            public Type Type { get; }
            public ComponentDescription(string key, string name, string category, Type type)
            {
                Key = key;
                Name = name;
                Category = category;
                Type = type;
            }
        }

        /// <summary>
        /// Determines whether the specified value is zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(this double value) => Math.Abs(value) < 1e-9;

        /// <summary>
        /// Parse a coordinate attribute on an XML node.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool ParseCoordinate(this XmlNode node, string attributeName, IDiagnosticHandler diagnostics, out double result)
        {
            string value = node.Attributes?[attributeName]?.Value;
            if (value == null)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected attribute '{attributeName}' on {node.Name}."));
                result = 0.0;
                return false;
            }
            if (!double.TryParse(value, NumberStyles.Float, Culture, out result))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected coordinate for '{attributeName}' on {node.Name}, but was '{value}'."));
                result = 0.0;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parse a vector attribute on an XML node.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="xAttribute">The name of the attribute representing the X-coordinate.</param>
        /// <param name="yAttribute">The name of the attribute representing the Y-coordinate.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool ParseVector(this XmlNode node, string xAttribute, string yAttribute, IDiagnosticHandler diagnostics, out Vector2 result)
        {
            bool success = true;
            success &= ParseCoordinate(node, xAttribute, diagnostics, out double x);
            success &= ParseCoordinate(node, yAttribute, diagnostics, out double y);
            result = new(x, y);
            return success;
        }

        /// <summary>
        /// Tries to parse an optional attribute.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryParseCoordinate(this XmlNode node, string attributeName, IDiagnosticHandler diagnostics, double defaultValue, out double result)
        {
            string value = node.Attributes?[attributeName]?.Value;
            if (value == null)
            {
                result = defaultValue;
                return false;
            }
            if (!double.TryParse(value, NumberStyles.Float, Culture, out result))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected coordinate for '{attributeName}' on {node.Name}, but was '{value}'."));
                result = defaultValue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to parse a vector attribute on an XML node.
        /// </summary>
        /// <param name="node">The XML node.</param>
        /// <param name="xAttribute">The name of the attribute representing the X-coordinate.</param>
        /// <param name="yAttribute">The name of the attribute representing the Y-coordinate.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryParseVector(this XmlNode node, string xAttribute, string yAttribute, IDiagnosticHandler diagnostics, Vector2 defaultValue, out Vector2 result)
        {
            bool success = true;
            success &= TryParseCoordinate(node, xAttribute, diagnostics, defaultValue.X, out double x);
            success &= TryParseCoordinate(node, yAttribute, diagnostics, defaultValue.Y, out double y);
            result = new(x, y);
            return success;
        }

        /// <summary>
        /// Gets all the component keys and types in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The components in the assembly.</returns>
        public static IEnumerable<ComponentDescription> Components(Assembly assembly)
        {
            yield break;
        }
    }
}
