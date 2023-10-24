using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.Variants;
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
        private readonly XmlNode _drawing, _pins;

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
            _scale = 1.0;
            string description = definition.Attributes["description"]?.Value ?? string.Empty;
            definition.Attributes["scale"]?.ParseScalar(diagnostics, out _scale, ErrorCodes.InvalidXmlScale);
            _metadata = new(new[] { key }, description, new[] { "Symbol" });

            _pins = definition.SelectSingleNode("pins");
            _drawing = definition.SelectSingleNode("drawing");
            if (_pins == null)
                diagnostics.Post(ErrorCodes.MissingSymbolPins, key);
            if (_drawing == null)
                diagnostics.Post(ErrorCodes.MissingSymbolDrawing, key);
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, _pins, _drawing, _scale);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly XmlNode _drawing, _pins;
            private readonly double _scale;
            private readonly List<int> _extend = new();

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
            public Instance(string type, string name, XmlNode pins, XmlNode drawing, double scale)
                : base(name)
            {
                Type = type;
                _scale = scale;
                _drawing = drawing;
                _pins = pins;
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;

                // Add the pins
                Pins.Clear();
                _extend.Clear();
                if (_pins != null)
                    AddPins(_pins, context);
                return true;
            }

            private void AddPins(XmlNode pins, IResetContext context)
            {
                foreach (XmlNode child in pins.ChildNodes)
                {
                    switch (child.Name)
                    {
                        case "pin":
                            {
                                // Read the pin properties
                                string name = child.Attributes["name"]?.Value;
                                string description = child.Attributes["description"]?.Value ?? "";
                                child.Attributes["x"].ParseScalar(context.Diagnostics, out double x);
                                child.Attributes["y"].ParseScalar(context.Diagnostics, out double y);
                                child.Attributes["nx"].ParseScalar(context.Diagnostics, out double nx);
                                child.Attributes["ny"].ParseScalar(context.Diagnostics, out double ny);
                                string extend = child.Attributes["extend"]?.Value ?? "false";
                                if (name == null)
                                {
                                    int c = Pins.Count;
                                    name = c.ToString();
                                    while (Pins.TryGetValue(name, out _))
                                    {
                                        c++;
                                        name = c.ToString();
                                    }
                                }
                                if (extend == "true")
                                    _extend.Add(Pins.Count);
                                if (nx.IsZero() && ny.IsZero())
                                    Pins.Add(new FixedPin(name, description, this, new(x, y)), name);
                                else
                                    Pins.Add(new FixedOrientedPin(name, description, this, new(x, y), new(nx, ny)), name);

                            }
                            break;

                        case "variant":
                            {
                                // Check if the variant matches
                                var value = child.Attributes["expression"]?.Value;
                                if (value == null)
                                    continue;
                                var lexer = new VariantLexer(value);
                                if (VariantParser.Parse(lexer, Variants))
                                    AddPins(child, context);
                            }
                            break;
                    }
                }
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (!_scale.Equals(1.0))
                    drawing.BeginTransform(new Transform(new(), Matrix2.Scale(_scale)));
                foreach (var pin in _extend)
                    drawing.ExtendPin(Pins[pin]);
                if (_drawing != null)
                {
                    var context = new XmlDrawingContext(Labels, Variants);
                    drawing.DrawXml(_drawing, context, drawing.Diagnostics);
                }
                if (!_scale.Equals(1.0))
                    drawing.EndTransform();
            }
        }
    }
}
