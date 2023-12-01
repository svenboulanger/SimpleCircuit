using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
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

        /// <inheritdoc />
        public IEnumerable<string> Keys => new string[] { _metadata.Key };

        /// <summary>
        /// Creates a new XML drawable.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        public XmlDrawable(string key, XmlNode definition, IDiagnosticHandler diagnostics)
        {
            // Extract the metadata
            string description = definition.Attributes["description"]?.Value ?? string.Empty;
            string category = definition.Attributes["category"]?.Value ?? "Custom";
            string keywords = definition.Attributes["keywords"]?.Value ?? string.Empty;

            _metadata = new(key, description, category);
            foreach (string keyword in keywords.Split(new[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries))
                _metadata.Keywords.Add(keyword);

            // Extract the pin definition
            _pins = definition.SelectSingleNode("pins");
            if (_pins == null)
                diagnostics.Post(ErrorCodes.MissingSymbolPins, key);

            // Extract the graphical definition
            _drawing = definition.SelectSingleNode("drawing");
            if (_drawing == null)
                diagnostics.Post(ErrorCodes.MissingSymbolDrawing, key);
        }

        /// <inheritdoc />
        public DrawableMetadata GetMetadata(string key)
        {
            if (key == _metadata.Key)
                return _metadata;
            return null;
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options, IDiagnosticHandler diagnostics)
            => new Instance(key, name, _pins, _drawing);

        /// <summary>
        /// Creates a new <see cref="Instance"/>
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="name">The instance name.</param>
        /// <param name="drawing">The XML data describing the node.</param>
        /// <param name="scale">The scale of the instance.</param>
        /// <param name="pins">The pins of the instance.</param>
        private class Instance(string type, string name, XmlNode pins, XmlNode drawing) : ScaledOrientedDrawable(name), ILabeled
        {
            private readonly XmlNode _drawing = drawing, _pins = pins;
            private readonly List<int> _extend = [];
            private readonly List<LabelAnchorPoint> _anchors = [];

            /// <inheritdoc />
            public override string Type { get; } = type;

            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
                    if (!EvaluateVariants(child.Attributes?["variant"]?.Value, Variants))
                        continue;

                    switch (child.Name)
                    {
                        case "pin":
                            {
                                // Read the pin properties
                                string name = child.Attributes?["name"]?.Value;
                                string description = child.Attributes["description"]?.Value ?? "";
                                child.Attributes.ParseOptionalScalar("x", context?.Diagnostics, 0.0, out double x);
                                child.Attributes.ParseOptionalScalar("y", context?.Diagnostics, 0.0, out double y);
                                child.Attributes.ParseOptionalScalar("nx", context?.Diagnostics, 0.0, out double nx);
                                child.Attributes.ParseOptionalScalar("ny", context?.Diagnostics, 0.0, out double ny);
                                string extend = child.Attributes?["extend"]?.Value;
                                string strAlias = child.Attributes?["alias"]?.Value;
                                var aliases = new List<string> { name };

                                if (!string.IsNullOrWhiteSpace(strAlias))
                                    aliases.AddRange(strAlias.Split(new char[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                                
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
                                else if (Pins.TryGetValue(name, out _))
                                {
                                    context?.Diagnostics?.Post(ErrorCodes.DuplicateSymbolPinName, name, Type);
                                    continue;
                                }
                                if (extend == "true")
                                    _extend.Add(Pins.Count);
                                if (nx.IsZero() && ny.IsZero())
                                    Pins.Add(new FixedPin(name, description, this, new(x, y)), aliases);
                                else
                                    Pins.Add(new FixedOrientedPin(name, description, this, new(x, y), new(nx, ny)), aliases);
                            }
                            break;

                        case "variant":
                        case "v":
                            AddPins(child, context);
                            break;
                    }
                }
            }

            private bool EvaluateVariants(string value, IVariantContext context)
            {
                if (value is null)
                    return true;
                if (string.IsNullOrWhiteSpace(value))
                    return false;
                var lexer = new VariantLexer(value);
                return VariantParser.Parse(lexer, context);
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                foreach (var pin in _extend)
                    drawing.ExtendPin(Pins[pin]);
                if (_drawing != null)
                {
                    var context = new XmlDrawingContext(Labels, Variants);
                    drawing.DrawXml(_drawing, context, drawing.Diagnostics);
                }
            }
        }
    }
}
