using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A drawable that is based on an XML description.
    /// </summary>
    public class XmlDrawable : IDrawableFactory
    {
        private readonly double _scale;
        private readonly DrawableMetadata _metadata;
        private readonly XmlNode _drawing;
        private readonly List<PinDescription> _pins = new();

        /// <inheritdoc />
        public IEnumerable<DrawableMetadata> Metadata
        {
            get
            {
                yield return _metadata;
            }
        }

        /// <summary>
        /// Creates a new XML drawable.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        public XmlDrawable(string key, XmlNode definition, IDiagnosticHandler diagnostics)
        {
            // Extract the metadata
            string description = "";
            _scale = 1.0;
            foreach (XmlAttribute attribute in definition.Attributes)
            {
                switch (attribute.Name)
                {
                    case "description": description = attribute.Value; break;
                    case "scale": attribute.ParseScalar(diagnostics, out _scale, ErrorCodes.InvalidXmlScale); break;
                    default:
                        diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                        break;
                }
            }
            _metadata = new(new[] { key }, description, new[] { "Symbol" });
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Build the pins
            int index = 0;
            foreach (XmlNode child in definition.ChildNodes)
            {
                switch (child.Name)
                {
                    case "pin":
                        string pinName = null;
                        description = "";
                        double x = 0, y = 0, nx = 0, ny = 0;
                        foreach (XmlAttribute attribute in child.Attributes)
                        {
                            switch (attribute.Name)
                            {
                                case "name": pinName = attribute.Value; break;
                                case "description": description = attribute.Value; break;
                                case "x": attribute.ParseScalar(diagnostics, out x); break;
                                case "y": attribute.ParseScalar(diagnostics, out y); break;
                                case "nx": attribute.ParseScalar(diagnostics, out nx); break;
                                case "ny": attribute.ParseScalar(diagnostics, out ny); break;

                                default:
                                    diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                                    break;

                            }
                        }
                        pinName ??= (index++).ToString();
                        if (usedNames.Add(pinName))
                        {
                            _pins.Add(new()
                            {
                                Name = pinName,
                                Description = description,
                                Location = new Vector2(x, y) * _scale,
                                Direction = new Vector2(nx, ny)
                            });
                        }
                        else
                            diagnostics?.Post(ErrorCodes.DuplicateSymbolPinName, pinName, key);
                        break;

                    case "drawing":
                        _drawing = child;
                        break;
                }
            }
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, _drawing, _scale, _pins);

        private class PinDescription
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Vector2 Location { get; set; }
            public Vector2 Direction { get; set; }
        }
        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly XmlNode _drawing;
            private readonly double _scale;

            /// <inheritdoc />
            public override string Type { get; }

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Creates a new <see cref="Instance"/>
            /// </summary>
            /// <param name="type">The instance type.</param>
            /// <param name="name">The instance name.</param>
            /// <param name="drawing">The XML data describing the node.</param>
            /// <param name="scale">The scale of the instance.</param>
            /// <param name="pins">The pins of the instance.</param>
            public Instance(string type, string name, XmlNode drawing, double scale, IEnumerable<PinDescription> pins)
                : base(name)
            {
                Type = type;
                _scale = scale;
                foreach (var pin in pins)
                {
                    if (pin.Direction.Equals(new Vector2()))
                        Pins.Add(new FixedPin(pin.Name, pin.Description, this, pin.Location), pin.Name);
                    else
                        Pins.Add(new FixedOrientedPin(pin.Name, pin.Description, this, pin.Location, pin.Direction), pin.Name);
                }
                _drawing = drawing;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (!_scale.Equals(1.0))
                    drawing.BeginTransform(new Transform(new(), Matrix2.Scale(_scale)));
                if (_drawing != null)
                {
                    var context = new XmlDrawingContext(Labels);
                    drawing.DrawXml(_drawing, context, drawing.Diagnostics);
                }
                if (!_scale.Equals(1.0))
                    drawing.EndTransform();
            }
        }
    }
}
