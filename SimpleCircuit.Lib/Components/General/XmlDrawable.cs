using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
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
            string description = definition.Attributes?["description"]?.Value ?? "";
            if (!double.TryParse(definition.Attributes?["scale"]?.Value ?? "1", out _scale))
            {
                diagnostics?.Post(ErrorCodes.SymbolScaleNotANumber, key);
                _scale = 1.0;
            }
            _metadata = new(new[] { key }, description, new[] { "Symbol" });

            // Build the pins
            int index = 0;
            foreach (XmlNode child in definition.ChildNodes)
            {
                switch (child.Name)
                {
                    case "pin":
                        string pinName = child.Attributes["name"]?.Value ?? (index++).ToString();
                        string pinDescription = child.Attributes["description"]?.Value ?? pinName;
                        if (!child.ParseVector("x", "y", diagnostics, out var location))
                            continue;
                        child.TryParseVector("nx", "ny", diagnostics, new(), out var direction);
                        _pins.Add(new()
                        {
                            Name = pinName,
                            Description = pinDescription,
                            Location = location * _scale,
                            Direction = direction
                        });
                        break;

                    case "drawing":
                        _drawing = child;
                        break;
                }
            }
        }

        /// <inheritdoc />
        public IDrawable Create(string key, string name, Options options)
            => new Instance(key, name, _drawing, _scale, _pins);

        private class PinDescription
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Vector2 Location { get; set; }
            public Vector2 Direction { get; set; }
        }
        private class Instance : ScaledOrientedDrawable
        {
            private readonly XmlNode _drawing;
            private readonly double _scale;

            /// <inheritdoc />
            public override string Type { get; }

            public Instance(string type, string name, XmlNode drawing, double scale, IEnumerable<PinDescription> pins)
                : base(name)
            {
                Type = type;
                _scale = scale;
                foreach (var pin in pins)
                {
                    if (pin.Direction.Equals(new Vector2()))
                        Pins.Add(new FixedPin(pin.Name, pin.Description, this, pin.Location));
                    else
                        Pins.Add(new FixedOrientedPin(pin.Name, pin.Description, this, pin.Location, pin.Direction), pin.Name);
                }
                _drawing = drawing;
            }

            protected override void Draw(SvgDrawing drawing)
            {
                if (!_scale.Equals(1.0))
                    drawing.BeginTransform(new Transform(new(), Matrix2.Scale(_scale)));
                if (_drawing != null)
                    drawing.DrawXml(_drawing, null);
                if (!_scale.Equals(1.0))
                    drawing.EndTransform();
            }
        }
    }
}
