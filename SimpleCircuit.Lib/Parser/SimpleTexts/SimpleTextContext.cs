using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
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
        private readonly XmlNode _parent;
        private XmlElement _currentSpan, _lineSpan;
        private readonly StringBuilder _sb = new();
        private readonly ITextMeasurer _measurer;
        private double _lastBaseLineOffset = 0.0;
        private double _lastRelativeFontWeight = 1.0;
        private readonly List<List<SimpleTextSpan>> _spans = new();

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the line spacing.
        /// </summary>
        public double LineSpacing { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets the base line offset for the next text.
        /// </summary>
        public double BaseLineOffset { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the relative font weight for the next text.
        /// </summary>
        public double RelativeFontWeight { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="SimpleTextContext"/>.
        /// </summary>
        public SimpleTextContext(XmlNode parent, ITextMeasurer measurer)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _document = parent.OwnerDocument;
            _measurer = measurer ?? new SkiaTextMeasurer("Tahoma");

            // Start a new line
            _lineSpan = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _currentSpan = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _lineSpan.AppendChild(_currentSpan);
            parent.AppendChild(_lineSpan);
            _spans.Add(new());
        }

        /// <summary>
        /// Appends text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Append(string text)
        {
            UpdateState();
            _sb.Append(text);
        }

        /// <summary>
        /// Appends a character.
        /// </summary>
        /// <param name="c">The character.</param>
        public void Append(char c)
        {
            UpdateState();
            _sb.Append(c);
        }

        private void UpdateState()
        {
            if (_lastBaseLineOffset != BaseLineOffset || _lastRelativeFontWeight != RelativeFontWeight)
            {
                FinishSpan();
                _lastBaseLineOffset = BaseLineOffset;
                _lastRelativeFontWeight = RelativeFontWeight;
            }
        }

        /// <summary>
        /// Adds a new line.
        /// </summary>
        public void FinishLine()
        {
            FinishSpan();

            // Start a fresh line
            _lineSpan = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _parent.AppendChild(_lineSpan);

            // Start a fresh span
            _currentSpan = _document.CreateElement("tspan", SvgDrawing.Namespace);
            _lineSpan.AppendChild(_currentSpan);

            // Add a new list of spans
            _spans.Add(new());

            // Restart
            _lastBaseLineOffset = 0.0;
            BaseLineOffset = 0.0;
            RelativeFontWeight = 1.0;
            _lastRelativeFontWeight = 1.0;
        }

        /// <summary>
        /// Finish a span within a line.
        /// </summary>
        private void FinishSpan()
        {
            if (_sb.Length == 0)
                return; // Nothing to add

            // Set the contents of the current span element
            string line = _sb.ToString();
            _currentSpan.InnerText = line;
            _sb.Clear();

            // Apply the styling to the span
            double size = Math.Round(FontSize * _lastRelativeFontWeight, 2);
            _currentSpan.SetAttribute("style", $"font-family: {_measurer.FontFamily}; font-size: {size}pt;");

            // Measure the text
            var bounds = _measurer.Measure(line, FontSize * _lastRelativeFontWeight);
            Vector2 offset;
            var lineSpans = _spans[^1];
            if (lineSpans.Count == 0)
                offset = new Vector2(0.0, _lastBaseLineOffset);
            else
            {
                var last = lineSpans[^1];
                offset = new Vector2(last.Delta.X + last.Bounds.Right, _lastBaseLineOffset);
            }
            lineSpans.Add(new SimpleTextSpan(_currentSpan, offset, bounds));

            // Make a new span
            _currentSpan = _document.CreateElement("tspan", _document.NamespaceURI);
            _lineSpan.AppendChild(_currentSpan);
        }

        /// <summary>
        /// Finishes the context.
        /// </summary>
        public Bounds Finish(Vector2 location, Vector2 expand)
        {
            FinishSpan();

            // Let's layout all these lines we got here
            ExpandableBounds bounds = new();
            double y = 0.0;
            foreach (var line in _spans)
            {
                foreach (var span in line)
                {
                    bounds.Expand(
                        new Vector2(span.Delta.X + span.Bounds.Left, y + span.Delta.Y + span.Bounds.Top),
                        new Vector2(span.Delta.X + span.Bounds.Right, y + span.Delta.Y + span.Bounds.Bottom));
                }
                y += FontSize * LineSpacing;
            }

            // Now that we have the total size, let's align correctly for the Y-axis.
            if (expand.Y.IsZero())
                y = location.Y - bounds.Bounds.Top - bounds.Bounds.Height * 0.5;
            else if (expand.Y < 0)
                y = location.Y - bounds.Bounds.Bottom;
            else
                y = location.Y - bounds.Bounds.Top;

            // Get the total bounds (which is easier to calculate right now)
            Bounds result;
            if (expand.X.IsZero())
            {
                result = new Bounds(
                    location.X - bounds.Bounds.Width * 0.5,
                    y + bounds.Bounds.Top,
                    location.X + bounds.Bounds.Width * 0.5,
                    y + bounds.Bounds.Bottom);
            }
            else if (expand.X < 0)
            {
                result = new Bounds(
                    location.X - bounds.Bounds.Width,
                    y + bounds.Bounds.Top,
                    location.X,
                    y + bounds.Bounds.Bottom);
            }
            else
            {
                result = new Bounds(
                    location.X,
                    y + bounds.Bounds.Top,
                    location.X + bounds.Bounds.Width,
                    y + bounds.Bounds.Bottom);
            }

            // Go through each line again, and align horizontally
            foreach (var line in _spans)
            {
                if (line.Count == 0)
                    continue;
                double left = line[0].Bounds.Left;
                double right = line[^1].Delta.X + line[^1].Bounds.Right;
                
                // Find the entry point of the text
                double x;
                if (expand.X.IsZero())
                    x = location.X - left - (right - left) * 0.5;
                else if (expand.X < 0)
                    x = location.X - right;
                else
                    x = location.X - left;

                // Update the positions
                foreach (var span in line)
                {
                    span.Element.SetAttribute("x", Convert(x + span.Delta.X));
                    span.Element.SetAttribute("y", Convert(y + span.Delta.Y));
                }
                y += FontSize * LineSpacing;
            }

            return result;
        }

        /// <summary>
        /// Converts a double to a rounded value for our svg-document.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private static string Convert(double value)
        {
            string result = Math.Round(value, 2).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            int length = result.Length - 1;
            while (result[length] == '0')
                length--;
            if (result[length] == '.')
                return result[..length];
            return result[..(length + 1)];
        }
    }
}
