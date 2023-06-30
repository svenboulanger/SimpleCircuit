using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.SimpleTexts;
using System;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// An abstract implementation of an element formatter that supports groups with graphic options.
    /// </summary>
    public abstract class BaseTextFormatter : ITextFormatter
    {
        /// <summary>
        /// Gets or sets the line spacing between lines of text.
        /// </summary>
        public double LineSpacing { get; set; } = 0.2;

        /// <inheritdoc />
        public Bounds Format(XmlNode parent, string text, Vector2 location, Vector2 expand, GraphicOptions options, IDiagnosticHandler diagnostics)
        {
            // Nothing to do?
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(text))
                return new(location, location);

            // Create a text element for formatting
            var textElement = parent.OwnerDocument.CreateElement("text", SvgDrawing.Namespace);
            options?.Apply(textElement);
            parent.AppendChild(textElement);

            // Create the elements for the text
            var lexer = new SimpleTextLexer(text);
            var context = new SimpleTextContext(textElement);
            SimpleTextParser.Parse(lexer, context);

            // Format spacing
            var bounds = new ExpandableBounds();
            var lineBounds = new Bounds[textElement.ChildNodes.Count];
            int index = 0;
            double height = 0;
            foreach (XmlElement line in textElement.ChildNodes)
            {
                // Format along X-axis
                var b = Measure(line);
                if (expand.X.IsZero())
                {
                    line.SetAttribute("text-anchor", "middle");
                    bounds.Expand(location - new Vector2(b.Width * 0.5, 0));
                    bounds.Expand(location + new Vector2(b.Width * 0.5, 0));
                }
                else if (expand.X > 0)
                {
                    line.SetAttribute("text-anchor", "start");
                    bounds.Expand(location);
                    bounds.Expand(location + new Vector2(b.Width, 0));
                }
                else
                {
                    line.SetAttribute("text-anchor", "end");
                    bounds.Expand(location - new Vector2(b.Width, 0));
                    bounds.Expand(location);
                }
                line.SetAttribute("x", Convert(location.X));

                // format along Y-axis: first determine the height of all lines
                if (height > 0)
                    height += LineSpacing;
                height += b.Height;
                lineBounds[index] = b;
                index++;
            }

            // Format the vertical spacing of the text lines
            double y;
            if (expand.Y.IsZero())
                y = location.Y -height * 0.5;
            else if (expand.Y > 0)
                y = location.Y;
            else
                y = location.Y - height;
            bounds.Expand(new Vector2(location.X, y));
            bounds.Expand(new Vector2(location.X, y + height));
            index = 0;
            foreach (XmlElement line in textElement.ChildNodes)
            {
                // Format along Y-axis: 
                y -= lineBounds[index].Top;
                line.SetAttribute("y", Convert(y));
                y += lineBounds[index].Bottom + LineSpacing;
                index++;
            }

            return bounds.Bounds;
        }

        /// <inheritdoc />
        public abstract Bounds Measure(XmlElement element);

        /// <summary>
        /// Converts a double to a rounded value for an svg-document.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        protected static string Convert(double value)
        {
            return Math.Round(value, 5).ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
