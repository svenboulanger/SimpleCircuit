using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SimpleCircuit
{
    /// <summary>
    /// An Svg drawing.
    /// </summary>
    public class SvgDrawing
    {
        /// <summary>
        /// The namespace for SVG nodes.
        /// </summary>
        public const string Namespace = "http://www.w3.org/2000/svg";

        private XmlElement _current;
        private readonly Stack<XmlElement> _group = new Stack<XmlElement>();
        private readonly Bounds _bounds;
        private readonly XmlDocument _document;

        /// <summary>
        /// Gets or sets the style of the drawing.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public string Style { get; set; }

        /// <summary>
        /// Gets the height of a line of text.
        /// </summary>
        /// <value>
        /// The height of a line of text.
        /// </value>
        public double LineHeight { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the width of a character.
        /// </summary>
        /// <value>
        /// The width of the character.
        /// </value>
        public double CharacterWidth { get; set; } = 3.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDrawing"/> class.
        /// </summary>
        public SvgDrawing()
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _bounds = new Bounds();
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="style">The style.</param>
        public void Line(Vector2 start, Vector2 end, string classes = null)
        {
            // Expand the bounds
            _bounds.Expand(start.X, start.Y);
            _bounds.Expand(end.X, end.Y);

            var line = _document.CreateElement("line", Namespace);
            line.SetAttribute("x1", Convert(start.X));
            line.SetAttribute("y1", Convert(start.Y));
            line.SetAttribute("x2", Convert(end.X));
            line.SetAttribute("y2", Convert(end.Y));
            if (!string.IsNullOrWhiteSpace(classes))
                line.SetAttribute("class", classes);

            _current.AppendChild(line);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="style">The style.</param>
        public void Circle(Vector2 position, double radius, string classes = null)
        {
            // Expand the bounds
            _bounds.Expand(position.X - radius, position.Y - radius);
            _bounds.Expand(position.X + radius, position.Y + radius);

            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", Convert(position.X));
            circle.SetAttribute("cy", Convert(position.Y));
            circle.SetAttribute("r", Convert(radius));
            if (!string.IsNullOrWhiteSpace(classes))
                circle.SetAttribute("class", classes);

            _current.AppendChild(circle);
        }

        /// <summary>
        /// Draws a polyline (connected lines).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void Polyline(IEnumerable<Vector2> points, string classes = null)
        {
            var sb = new StringBuilder(32);
            bool isFirst = true;
            foreach (var point in points)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" ");
                _bounds.Expand(point.X, point.Y);
                sb.Append($"{Convert(point.X)},{Convert(point.Y)}");
            }

            var poly = _document.CreateElement("polyline", Namespace);
            poly.SetAttribute("points", sb.ToString());
            if (!string.IsNullOrWhiteSpace(classes))
                poly.SetAttribute("class", classes);

            _current.AppendChild(poly);
        }

        /// <summary>
        /// Draws a polygon (a closed shape of straight lines).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void Polygon(IEnumerable<Vector2> points, string classes = null)
        {
            var sb = new StringBuilder(32);
            bool isFirst = true;
            foreach (var point in points)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" ");
                _bounds.Expand(point.X, point.Y);
                sb.Append($"{Convert(point.X)},{Convert(point.Y)}");
            }

            var poly = _document.CreateElement("polygon", Namespace);
            poly.SetAttribute("points", sb.ToString());
            if (!string.IsNullOrWhiteSpace(classes))
                poly.SetAttribute("class", classes);

            _current.AppendChild(poly);
        }

        /// <summary>
        /// Draws multiple lines. Every two points define a separate line.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="style">The style.</param>
        public void Segments(IEnumerable<Vector2> points, string classes = null)
        {
            var sb = new StringBuilder(128);
            var it = points.GetEnumerator();
            while (it.MoveNext())
            {
                var first = it.Current;
                if (!it.MoveNext())
                    return;
                var second = it.Current;
                _bounds.Expand(first);
                _bounds.Expand(second);
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append($"M {Convert(first.X)} {Convert(first.Y)} L {Convert(second.X)} {Convert(second.Y)}");
            }

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            if (!string.IsNullOrWhiteSpace(classes))
                path.SetAttribute("class", classes);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Draws a smooth bezier curve.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void SmoothBezier(IEnumerable<Vector2> points, string classes = null)
        {
            var sb = new StringBuilder(128);
            var it = points.GetEnumerator();
            bool isFirst = true;
            Vector2 end;
            while (it.MoveNext())
            {
                if (isFirst)
                {
                    Vector2 first = it.Current;
                    if (!it.MoveNext())
                        return;

                    // Get the handles
                    var handle1 = it.Current;
                    if (!it.MoveNext())
                        return;
                    var handle2 = it.Current;
                    if (!it.MoveNext())
                        return;
                    end = it.Current;

                    // Expand bounds
                    _bounds.Expand(first);
                    _bounds.Expand(handle1);
                    _bounds.Expand(handle2);
                    _bounds.Expand(end);

                    // Add the data
                    sb.Append($"M{Convert(first.X)} {Convert(first.Y)} ");
                    sb.Append($"C{Convert(handle1.X)} {Convert(handle1.Y)}, ");
                    sb.Append($"{Convert(handle2.X)} {Convert(handle2.Y)}, ");
                    sb.Append($"{Convert(end.X)} {Convert(end.Y)}");

                    isFirst = false;
                }
                else
                {
                    var handle = it.Current;
                    if (!it.MoveNext())
                        return;
                    end = it.Current;

                    _bounds.Expand(handle);
                    _bounds.Expand(end);

                    // Add the data
                    sb.Append($" S{Convert(handle.X)} {Convert(handle.Y)} ");
                    sb.Append($"{Convert(end.X)} {Convert(end.Y)}");
                }
            }

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            if (!string.IsNullOrWhiteSpace(classes))
                path.SetAttribute("class", classes);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The direction of the quadrant that the text can expand to.</param>
        /// <param name="classes">The classes.</param>
        public void Text(string value, Vector2 location, Vector2 expand, double fontSize = 4, string classes = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            var lines = value.Split(new char[] { '\r', '\n', '\\' });
            var width = 0.0;
            foreach (var l in lines)
                width = Math.Max(width, l.Length * CharacterWidth);
            var height = lines.Length * LineHeight;

            // Create the text element
            var text = _document.CreateElement("text", Namespace);

            // Expand the X-direction
            if (expand.X.IsZero())
            {
                text.SetAttribute("text-anchor", "middle");
                _bounds.Expand(location - new Vector2(width / 2, 0));
                _bounds.Expand(location + new Vector2(width / 2, 0));
            }
            else if (expand.X > 0)
            {
                text.SetAttribute("text-anchor", "start");
                _bounds.Expand(location);
                _bounds.Expand(location + new Vector2(width, 0));
            }
            else
            {
                text.SetAttribute("text-anchor", "end");
                _bounds.Expand(location - new Vector2(width, 0));
                _bounds.Expand(location);
            }

            // Draw the text with multiple lines
            if (expand.Y.IsZero())
            {
                _bounds.Expand(location - new Vector2(0, LineHeight * lines.Length / 2));
                _bounds.Expand(location + new Vector2(0, LineHeight * lines.Length / 2));

                // Center everything
                var dy = -(height - LineHeight) * 0.5;
                foreach (var l in lines)
                {
                    var tspan = _document.CreateElement("tspan", Namespace);
                    tspan.InnerText = l;
                    tspan.SetAttribute("y", Convert(location.Y + dy + fontSize / 3.0));
                    tspan.SetAttribute("x", Convert(location.X));
                    text.AppendChild(tspan);
                    dy += LineHeight;
                }

            }
            else if (expand.Y > 0)
            {
                _bounds.Expand(location);
                _bounds.Expand(location + new Vector2(0, LineHeight * lines.Length));

                // Make sure everything is below
                double dy = 0;
                foreach (var l in lines)
                {
                    var tspan = _document.CreateElement("tspan", Namespace);
                    tspan.InnerText = l;
                    tspan.SetAttribute("y", Convert(location.Y + dy + fontSize));
                    tspan.SetAttribute("x", Convert(location.X));
                    text.AppendChild(tspan);
                    dy += LineHeight;
                }
            }
            else
            {
                _bounds.Expand(location - new Vector2(0, LineHeight * lines.Length));
                _bounds.Expand(location);

                // Make sure everything is above
                double dy = -height + LineHeight;
                foreach (var l in lines)
                {
                    var tspan = _document.CreateElement("tspan", Namespace);
                    tspan.InnerText = l;
                    tspan.SetAttribute("y", Convert(location.Y + dy));
                    tspan.SetAttribute("x", Convert(location.X));
                    text.AppendChild(tspan);
                    dy += LineHeight;
                }
            }

            text.SetAttribute("x", Convert(location.X));
            text.SetAttribute("y", Convert(location.Y));
            text.SetAttribute("font-size", $"{Convert(fontSize)}pt");

            if (!string.IsNullOrWhiteSpace(classes))
                text.SetAttribute("class", classes);
            _current.AppendChild(text);
        }

        /// <summary>
        /// Starts a group.
        /// </summary>
        /// <param name="id">The identifier of the group.</param>
        /// <param name="classes">The classes.</param>
        public void StartGroup(string id, string classes = null)
        {
            var elt = _document.CreateElement("g", Namespace);
            if (!string.IsNullOrEmpty(id))
                elt.SetAttribute("id", id);
            if (!string.IsNullOrWhiteSpace(classes))
                elt.SetAttribute("class", classes);
            _group.Push(_current);
            _current = elt;
        }

        /// <summary>
        /// Ends the last opened group.
        /// </summary>
        public void EndGroup()
        {
            if (_group.Count > 0)
            {
                var parent = _group.Pop();
                parent.AppendChild(_current);
                _current = parent;
            }
        }

        /// <summary>
        /// Gets the SVG xml-document.
        /// </summary>
        /// <returns>The document.</returns>
        public XmlDocument GetDocument()
        {
            _current.SetAttribute("width", ((int)(_bounds.Width * 5)).ToString());
            _current.SetAttribute("height", ((int)(_bounds.Height * 5)).ToString());
            _current.SetAttribute("viewBox", $"{Convert(_bounds.Left)} {Convert(_bounds.Top)} {Convert(_bounds.Width)} {Convert(_bounds.Height)}");

            // Add stylesheet info if necessary
            if (!string.IsNullOrWhiteSpace(Style))
            {
                var style = _document.CreateElement("style", Namespace);
                style.InnerText = Style;
                _current.PrependChild(style);
            }

            return _document;
        }

        /// <summary>
        /// Converts a double to a rounded value for our svg-document.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private string Convert(double value)
        {
            return Math.Round(value, 5).ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
