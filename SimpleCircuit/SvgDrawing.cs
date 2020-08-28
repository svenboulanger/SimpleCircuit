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
        private XmlElement _current;
        private readonly Stack<XmlElement> _group = new Stack<XmlElement>();
        private readonly Bounds _bounds;
        private readonly XmlDocument _document;

        public enum Extend
        {
            Left,
            Right,
            Top,
            Bottom
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDrawing"/> class.
        /// </summary>
        public SvgDrawing()
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", "http://www.w3.org/2000/svg");
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

            var line = _document.CreateElement("line");
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

            var circle = _document.CreateElement("circle");
            circle.SetAttribute("cx", Convert(position.X));
            circle.SetAttribute("cy", Convert(position.Y));
            circle.SetAttribute("r", Convert(radius));
            if (!string.IsNullOrWhiteSpace(classes))
                circle.SetAttribute("class", classes);

            _current.AppendChild(circle);
        }

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="style">The style.</param>
        public void Poly(IEnumerable<Vector2> points, string classes = null)
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

            var poly = _document.CreateElement("polyline");
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

            var path = _document.CreateElement("path");
            path.SetAttribute("d", sb.ToString());
            if (!string.IsNullOrWhiteSpace(classes))
                path.SetAttribute("class", classes);
            _current.AppendChild(path);
        }

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

                    // Get the first handle
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

            var path = _document.CreateElement("path");
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
        /// <param name="expand">The expand.</param>
        /// <param name="classes">The classes.</param>
        public void Text(string value, Vector2 location, Vector2 expand, string classes = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            var text = _document.CreateElement("text");
            text.SetAttribute("x", Convert(location.X));
            text.SetAttribute("y", Convert(location.Y));

            List<string> styles = new List<string>();
            if (expand.X.IsZero())
            {
                styles.Add("text-anchor: middle;");
                _bounds.Expand(location - new Vector2(value.Length * 5, 0));
                _bounds.Expand(location + new Vector2(value.Length * 5, 0));
            }
            else if (expand.X > 0)
            {
                styles.Add("text-anchor: start;");
                _bounds.Expand(location);
                _bounds.Expand(location + new Vector2(value.Length * 10, 0));
            }
            else
            {
                styles.Add("text-anchor: end;");
                _bounds.Expand(location - new Vector2(value.Length * 10, 0));
                _bounds.Expand(location);
            }
            if (expand.Y.IsZero())
            {
                styles.Add("dominant-baseline: middle;");
                _bounds.Expand(location - new Vector2(0, 5));
                _bounds.Expand(location + new Vector2(0, 5));
            }
            else if (expand.Y > 0)
            {
                styles.Add("dominant-baseline: hanging;");
                _bounds.Expand(location);
                _bounds.Expand(location + new Vector2(0, 10));
            }
            else
            {
                styles.Add("dominant-baseline: auto;");
                _bounds.Expand(location - new Vector2(0, 10));
                _bounds.Expand(location);
            }
            text.SetAttribute("style", string.Join(" ", styles));
            if (!string.IsNullOrWhiteSpace(classes))
                text.SetAttribute("class", classes);
            text.InnerText = value;
            _current.AppendChild(text);
        }

        /// <summary>
        /// Starts a group.
        /// </summary>
        /// <param name="id">The identifier of the group.</param>
        /// <param name="classes">The classes.</param>
        public void StartGroup(string id, string classes = null)
        {
            var elt = _document.CreateElement("g");
            if (!string.IsNullOrEmpty(id))
                elt.SetAttribute("id", id);
            if (!string.IsNullOrWhiteSpace(classes))
                elt.SetAttribute("class", classes);
            _group.Push(_current);
            _current = elt;
        }

        /// <summary>
        /// Ends the group.
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
