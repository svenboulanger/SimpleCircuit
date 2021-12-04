using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Globalization;
using System.Xml;

namespace SimpleCircuit.Components.General
{
    public class XmlDrawable : ScaledOrientedDrawable
    {
        private readonly static IFormatProvider _culture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Creates a new drawable based on an XML description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="definition"></param>
        /// <param name="diagnostics"></param>
        /// <param name="options"></param>
        public XmlDrawable(string name, XmlNode definition, IDiagnosticHandler diagnostics, Options options)
            : base(name, options)
        {
            // Build the pins
            int index = 0;
            foreach (XmlNode child in definition.ChildNodes)
            {
                switch (child.Name)
                {
                    case "pin":
                        string pinName = child.Attributes["name"]?.Value ?? (index++).ToString();
                        string pinDescription = child.Attributes["description"]?.Value ?? pinName;
                        if (!ParseVector(child, "x", "y", diagnostics, out var location))
                            continue;
                        ParseVector(child, "nx", "ny", diagnostics, out var direction);

                        if (direction.Equals(new Vector2()))
                            Pins.Add(new FixedPin(pinName, pinDescription, this, location));
                        else
                            Pins.Add(new FixedOrientedPin(pinName, pinDescription, this, location, direction));
                        break;

                    case "drawing":
                        DrawingVariants = Variant.Do((SvgDrawing drawing) => drawing.DrawXml(child, diagnostics));
                        break;
                }
            }
        }

        private bool ParseCoordinate(string text, IDiagnosticHandler diagnostics, out double result)
        {
            if (!double.TryParse(text, NumberStyles.Float, _culture, out result))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Cannot recognize startin coordinate '{text}'."));
                return false;
            }
            return true;
        }
        private bool ParseVector(XmlNode node, string xAttribute, string yAttribute, IDiagnosticHandler diagnostics, out Vector2 result)
        {
            if (!ParseCoordinate(node.Attributes[xAttribute]?.Value ?? "", diagnostics, out double x) ||
                !ParseCoordinate(node.Attributes[yAttribute]?.Value ?? "", diagnostics, out double y))
            {
                result = new();
                return false;
            }
            result = new(x, y);
            return true;
        }
    }
}
