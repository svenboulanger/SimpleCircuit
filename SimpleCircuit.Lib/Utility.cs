using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.SvgPathData;
using System;
using System.Globalization;
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

        /// <summary>
        /// Determines whether the specified value is zero.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// Returns <c>true</c> if the specified value is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(this double value) => Math.Abs(value) < 1e-9;

        /// <summary>
        /// Determines whether the specified vector is zero.
        /// </summary>
        /// <param name="vector">The value.</param>
        /// <returns>
        /// Return <c>true</c> if the specified value is zero; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsZero(this Vector2 vector) => vector.X.IsZero() && vector.Y.IsZero();

        public static bool IsNaN(this double value) => double.IsNaN(value);

        /// <summary>
        /// Determines whether the specified vector has a coordinate that is not a number.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>
        /// Returns <c>true</c> if one of the vector coordinates is not a number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNaN(this Vector2 vector) => vector.X.IsNaN() || vector.Y.IsNaN();

        /// <summary>
        /// Parses a scalar attribute of an XML ndoe.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool ParseScalar(this XmlAttribute attribute, IDiagnosticHandler diagnostics, out double result, ErrorCodes errorCode = ErrorCodes.InvalidXmlCoordinate)
        {
            if (attribute == null)
            {
                result = 0.0;
                return false;
            }
            if (!double.TryParse(attribute.Value, NumberStyles.Float, Culture, out result))
            {
                diagnostics?.Post(errorCode, attribute.Value, attribute.ParentNode.Name);
                result = 0.0;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses an optional scalar value.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defValue">The default value.</param>
        /// <param name="result">The result.</param>
        /// <param name="errorCode">An optional error code for invalid scalars.</param>
        /// <returns>Returns <c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public static bool ParseOptionalScalar(this XmlAttributeCollection attributes, string name, IDiagnosticHandler diagnostics, double defValue, out double result, ErrorCodes errorCode = ErrorCodes.InvalidXmlCoordinate)
        {
            if (attributes == null)
            {
                result = defValue;
                return true;
            }

            var attribute = attributes[name];
            if (attribute == null)
            {
                result = defValue;
                return true;
            }

            if (!double.TryParse(attribute.Value, NumberStyles.Float, Culture, out result))
            {
                diagnostics?.Post(errorCode, attribute, attribute.ParentNode.Name);
                result = defValue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses an optional vector value.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defValue">The default value.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
        public static bool ParseOptionalVector(this XmlAttributeCollection attributes, string name, IDiagnosticHandler diagnostics, Vector2 defValue, out Vector2 result)
        {
            if (attributes == null)
            {
                result = defValue;
                return true;
            }

            var attribute = attributes[name];
            if (attribute == null)
            {
                result = defValue;
                return true;
            }

            var lexer = new SvgPathDataLexer(attribute.Value);
            if (!lexer.ParseVector(diagnostics, out result))
            {
                result = defValue;
                return false;
            }
            return true;
        }

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
                diagnostics?.Post(ErrorCodes.ExpectedAttributeOn, attributeName, node.Name);
                result = 0.0;
                return false;
            }
            if (!double.TryParse(value, NumberStyles.Float, Culture, out result))
            {
                diagnostics?.Post(ErrorCodes.ExpectedCoordinateForOnButWas, attributeName, node.Name, value);
                result = 0.0;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to parse an optional coordinate.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool ParseCoordinate(this SvgPathDataLexer lexer, IDiagnosticHandler diagnostics, out double result)
        {
            if (lexer.Branch(TokenType.Number, out var value))
            {
                if (!double.TryParse(value.Content.ToString(), NumberStyles.Float, Culture, out result))
                {
                    diagnostics?.Post(value, ErrorCodes.ExpectedCoordinateButWas, value.Content);
                    return false;
                }
                return true;
            }
            else
            {
                result = 0.0;
                diagnostics?.Post(lexer.NextToken, ErrorCodes.ExpectedCoordinateButWas, lexer.NextToken.Content);
                return false;
            }
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
        /// Parse a vector attribute on an XML node.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool ParseVector(this SvgPathDataLexer lexer, IDiagnosticHandler diagnostics, out Vector2 result)
        {
            bool success = true;
            success &= ParseCoordinate(lexer, diagnostics, out double x);
            success &= ParseCoordinate(lexer, diagnostics, out double y);
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
                diagnostics?.Post(ErrorCodes.ExpectedCoordinateForOnButWas, attributeName, node.Name, value);
                result = defaultValue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to parse an optional coordinate.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryParseCoordinate(this SvgPathDataLexer lexer, IDiagnosticHandler diagnostics, double defaultValue, out double result)
        {
            if (lexer.Branch(TokenType.Number, out var value))
            {
                if (!double.TryParse(value.Content.ToString(), NumberStyles.Float, Culture, out result))
                {
                    diagnostics?.Post(value, ErrorCodes.ExpectedCoordinateButWas, value.Content);
                    return false;
                }
                return true;
            }
            else
            {
                result = defaultValue;
                return false;
            }
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
        /// Tries to parse a vector attribute on an XML node.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the coordinate was parsed; otherwise, <c>false</c>.</returns>
        public static bool TryParseVector(this SvgPathDataLexer lexer, IDiagnosticHandler diagnostics, Vector2 defaultValue, out Vector2 result)
        {
            bool success = true;
            success &= TryParseCoordinate(lexer, diagnostics, defaultValue.X, out double x);
            success &= TryParseCoordinate(lexer, diagnostics, defaultValue.Y, out double y);
            result = new(x, y);
            return success;
        }

        /// <summary>
        /// Converts a scalar value to a coordinate for SVG. It will automatically round the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representing the value.</returns>
        public static string ToSVG(this double value)
        {
            string result = Math.Round(value, 2).ToString("F2", CultureInfo.InvariantCulture);
            int length = result.Length - 1;
            while (result[length] == '0')
                length--;
            if (result[length] == '.')
                return result[..length];
            return result[..(length + 1)];
        }

        /// <summary>
        /// Converts a vector to a coordinate for SVG. It will automatically round the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representing the value.</returns>
        public static string ToSVG(this Vector2 value)
            => $"{value.X.ToSVG()},{value.Y.ToSVG()}";
    }
}
