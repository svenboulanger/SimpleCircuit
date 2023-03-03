using System;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A context for parsing SimpleCircuit text.
    /// </summary>
    public class SimpleTextContext
    {
        private readonly XmlDocument _document;
        private XmlElement _current, _line;
        private readonly StringBuilder _sb = new();
        private double _dy = 0.0;

        /// <summary>
        /// Gets the current node to add spans to.
        /// </summary>
        public XmlNode Parent { get; }

        /// <summary>
        /// Gets the base line y.
        /// </summary>
        public double BaselineY { get; set; }

        /// <summary>
        /// Gets or sets the relative font weight.
        /// </summary>
        public double RelativeFontWeight { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="SimpleTextContext"/>.
        /// </summary>
        public SimpleTextContext(XmlNode parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _document = parent.OwnerDocument;
            _line = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _current = _document.CreateElement("tspan", SvgDrawing.Namespace);
            parent.AppendChild(_line);
            _line.AppendChild(_current);
        }

        /// <summary>
        /// Appends text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Append(string text)
        {
            UpdateBaseline();
            _sb.Append(text);
        }

        /// <summary>
        /// Appends a character.
        /// </summary>
        /// <param name="c">The character.</param>
        public void Append(char c)
        {
            UpdateBaseline();
            _sb.Append(c);
        }

        private void UpdateBaseline()
        {
            if (_dy != BaselineY)
            {
                // Stop the last tspan and start a new one with the new settings
                _current.InnerXml = _sb.ToString();
                _sb.Clear();

                // The font size is important here...
                double dy = (BaselineY - _dy) / RelativeFontWeight;
                _current = _document.CreateElement("tspan", SvgDrawing.Namespace);
                _current.SetAttribute("dy", $"{dy:G3}em");
                if (RelativeFontWeight != 1.0)
                    _current.SetAttribute("style", $"font-size: {RelativeFontWeight:G3}em");
                _dy = BaselineY;
            }
        }

        /// <summary>
        /// Adds a new line.
        /// </summary>
        public void Newline()
        {
            // Stop the current tspan
            _current.InnerXml = _sb.ToString();
            _sb.Clear();

            // Start fresh
            _line = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _current = _document.CreateElement("tspan", SvgDrawing.Namespace);
            Parent.AppendChild(_line);
            _line.AppendChild(_current);
            RelativeFontWeight = 1.0;
            _dy = 0;
        }

        /// <summary>
        /// Finishes the context.
        /// </summary>
        public void Finish()
        {
            _current.InnerXml = _sb.ToString();
            _current = null;
            _line = null;
        }

        /// <summary>
        /// Clears the context.
        /// </summary>
        public void Clear()
        {
            _sb.Clear();
            _dy = BaselineY = 0.0;
            RelativeFontWeight = 1.0;
        }
    }
}
