using SimpleCircuit.Drawing;
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
        public const string SimpleCircuitNamespace = "https://github.com/svenboulanger/SimpleCircuit";

        private XmlElement _current;
        private readonly Stack<XmlElement> _group = new();
        private readonly ExpandableBounds _bounds;
        private readonly XmlDocument _document;
        private readonly Stack<Transform> _tf = new();

        /// <summary>
        /// Gets the current transform.
        /// </summary>
        /// <value>
        /// The current transform.
        /// </value>
        protected Transform CurrentTransform => _tf.Peek();

        /// <summary>
        /// Gets or sets the style of the drawing.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public string Style { get; set; } = 
@"path, polyline, line, circle, polygon {
    stroke: black;
    stroke-width: 0.5pt;
    fill: none;
    stroke-linecap: round;
    stroke-linejoin: round;
}
.point circle { fill: black; }
.plane { stroke-width: 1pt; }
text { font-family: Tahoma, Verdana, Segoe, sans-serif; }";

        /// <summary>
        /// Gets or sets the text formatter.
        /// </summary>
        public ITextFormatter TextFormatter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDrawing"/> class.
        /// </summary>
        public SvgDrawing()
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _bounds = new ExpandableBounds();
            _tf.Push(Transform.Identity);
        }

        /// <summary>
        /// Begins a new transform on top of previous transforms.
        /// </summary>
        /// <param name="tf">The transform.</param>
        public void BeginTransform(Transform tf)
        {
            // _tf.Push(tf.Apply(TF));
            _tf.Push(tf.Apply(CurrentTransform));
        }

        /// <summary>
        /// Ends the last transform transform.
        /// </summary>
        public void EndTransform()
        {
            _tf.Pop();
            if (_tf.Count == 0)
                _tf.Push(Transform.Identity);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="options">Optional options.</param>
        public void Line(Vector2 start, Vector2 end, PathOptions options = null)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);

            // Expand the bounds
            _bounds.Expand(start.X, start.Y);
            _bounds.Expand(end.X, end.Y);

            var line = _document.CreateElement("line", Namespace);
            line.SetAttribute("x1", Convert(start.X));
            line.SetAttribute("y1", Convert(start.Y));
            line.SetAttribute("x2", Convert(end.X));
            line.SetAttribute("y2", Convert(end.Y));
            options?.Apply(line);
            _current.AppendChild(line);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="options">The options.</param>
        public void Circle(Vector2 position, double radius, GraphicOptions options = null)
        {
            // Let's see if there is some scaling involved
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            position = CurrentTransform.Apply(position);

            // Expand the bounds
            _bounds.Expand(position.X - radius, position.Y - radius);
            _bounds.Expand(position.X + radius, position.Y + radius);

            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", Convert(position.X));
            circle.SetAttribute("cy", Convert(position.Y));
            circle.SetAttribute("r", Convert(radius));
            options?.Apply(circle);
            _current.AppendChild(circle);
        }

        /// <summary>
        /// Draws a polyline (connected lines).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void Polyline(IEnumerable<Vector2> points, PathOptions options = null)
        {
            var sb = new StringBuilder(32);
            bool isFirst = true;
            foreach (var point in points)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" ");
                var pt = CurrentTransform.Apply(point);
                _bounds.Expand(pt.X, pt.Y);
                sb.Append($"{Convert(pt.X)},{Convert(pt.Y)}");
            }

            var poly = _document.CreateElement("polyline", Namespace);
            poly.SetAttribute("points", sb.ToString());
            options?.Apply(poly);
            _current.AppendChild(poly);
        }

        /// <summary>
        /// Draws a polygon (a closed shape of straight lines).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The options.</param>
        public void Polygon(IEnumerable<Vector2> points, PathOptions options = null)
        {
            var sb = new StringBuilder(32);
            bool isFirst = true;
            foreach (var point in points)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" ");
                var pt = CurrentTransform.Apply(point);
                _bounds.Expand(pt.X, pt.Y);
                sb.Append($"{Convert(pt.X)},{Convert(pt.Y)}");
            }

            var poly = _document.CreateElement("polygon", Namespace);
            poly.SetAttribute("points", sb.ToString());
            options?.Apply(poly);
            _current.AppendChild(poly);
        }

        /// <summary>
        /// Draws multiple lines. Every two points define a separate line.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The options.</param>
        public void Segments(IEnumerable<Vector2> points, PathOptions options = null)
        {
            var sb = new StringBuilder(128);
            var it = points.GetEnumerator();
            while (it.MoveNext())
            {
                var first = it.Current;
                if (!it.MoveNext())
                    return;
                var second = it.Current;

                first = CurrentTransform.Apply(first);
                second = CurrentTransform.Apply(second);
                _bounds.Expand(first);
                _bounds.Expand(second);
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append($"M {Convert(first.X)} {Convert(first.Y)} L {Convert(second.X)} {Convert(second.Y)}");
            }

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            options?.Apply(path);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Draws a smooth bezier curve.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void SmoothBezier(IEnumerable<Vector2> points, PathOptions options = null)
        {
            var sb = new StringBuilder(128);
            var it = points.GetEnumerator();
            bool isFirst = true;
            Vector2 end;
            while (it.MoveNext())
            {
                if (isFirst)
                {
                    Vector2 first = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        return;

                    // Get the handles
                    var handle1 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        return;
                    var handle2 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        return;
                    end = CurrentTransform.Apply(it.Current);

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
                    var handle = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        return;
                    end = CurrentTransform.Apply(it.Current);

                    _bounds.Expand(handle);
                    _bounds.Expand(end);

                    // Add the data
                    sb.Append($" S{Convert(handle.X)} {Convert(handle.Y)} ");
                    sb.Append($"{Convert(end.X)} {Convert(end.Y)}");
                }
            }

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            options?.Apply(path);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Closed bezier curve.
        /// </summary>
        /// <param name="pointsAndHandles">The points and handles.</param>
        /// <param name="options">The options.</param>
        public void ClosedBezier(IEnumerable<Vector2> pointsAndHandles, PathOptions options = null)
        {
            var sb = new StringBuilder(128);
            var it = pointsAndHandles.GetEnumerator();
            bool isFirst = true;
            while (it.MoveNext())
            {
                if (isFirst)
                {
                    var m = CurrentTransform.Apply(it.Current);
                    sb.Append($"M{Convert(m.X)} {Convert(m.Y)} ");
                    _bounds.Expand(m);
                    isFirst = false;
                }
                else
                {
                    var h1 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        break;
                    var h2 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        break;
                    var end = CurrentTransform.Apply(it.Current);
                    sb.Append($"C{Convert(h1.X)} {Convert(h1.Y)}, {Convert(h2.X)} {Convert(h2.Y)}, {Convert(end.X)} {Convert(end.Y)} ");
                    _bounds.Expand(h1);
                    _bounds.Expand(h2);
                    _bounds.Expand(end);
                }
            }

            // Close the path
            sb.Append("Z");

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            options?.Apply(path);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Open bezier curve.
        /// </summary>
        /// <param name="pointsAndHandles">The points and handles.</param>
        /// <param name="options">The options.</param>
        public void OpenBezier(IEnumerable<Vector2> pointsAndHandles, PathOptions options = null)
        {
            var sb = new StringBuilder(128);
            var it = pointsAndHandles.GetEnumerator();
            bool isFirst = true;
            while (it.MoveNext())
            {
                if (isFirst)
                {
                    var m = CurrentTransform.Apply(it.Current);
                    sb.Append($"M{Convert(m.X)} {Convert(m.Y)} ");
                    _bounds.Expand(m);
                    isFirst = false;
                }
                else
                {
                    var h1 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        break;
                    var h2 = CurrentTransform.Apply(it.Current);
                    if (!it.MoveNext())
                        break;
                    var end = CurrentTransform.Apply(it.Current);
                    sb.Append($"C{Convert(h1.X)} {Convert(h1.Y)}, {Convert(h2.X)} {Convert(h2.Y)}, {Convert(end.X)} {Convert(end.Y)} ");
                    _bounds.Expand(h1);
                    _bounds.Expand(h2);
                    _bounds.Expand(end);
                }
            }

            var path = _document.CreateElement("path", Namespace);
            path.SetAttribute("d", sb.ToString());
            options?.Apply(path);
            _current.AppendChild(path);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The direction of the quadrant that the text can expand to.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="midLineFactor">The mid line factor for centering text.</param>
        /// <param name="options">The options.</param>
        public void Text(string value, Vector2 location, Vector2 expand, double fontSize = 4, double midLineFactor = 0.33, GraphicOptions options = null)
        {
            if (TextFormatter == null)
                TextFormatter = new TextFormatter();
            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            if (string.IsNullOrWhiteSpace(value))
                return;
            var lines = value.Split(new char[] { '\r', '\n', '\\' });
            List<FormattedText> formattedLines = new();
            double width = 0, height = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = TextFormatter.Format(lines[i], fontSize);
                lines[i] = t.Content;
                width = Math.Max(t.Bounds.Width, width);
                height += t.Bounds.Height;
                formattedLines.Add(t);
            }

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
            double y = 0.0;
            if (expand.Y.IsZero())
            {
                _bounds.Expand(location - new Vector2(0, height / 2));
                _bounds.Expand(location + new Vector2(0, height / 2));
                y = -height / 2;
            }
            else if (expand.Y > 0)
            {
                _bounds.Expand(location);
                _bounds.Expand(location + new Vector2(0, height));
                y = 0.0;
            }
            else
            {
                _bounds.Expand(location - new Vector2(0, height));
                _bounds.Expand(location);
                y = -height;
            }

            // Make sure everything is above
            foreach (var l in formattedLines)
            {
                // Go to baseline of text
                y -= l.Bounds.Top;
                var tspan = _document.CreateElement("tspan", Namespace);
                tspan.InnerXml = l.Content;
                tspan.SetAttribute("y", Convert(location.Y + y));
                tspan.SetAttribute("x", Convert(location.X));
                text.AppendChild(tspan);

                // Space below text
                y += l.Bounds.Bottom;
            }

            text.SetAttribute("x", Convert(location.X));
            text.SetAttribute("y", Convert(location.Y));
            text.SetAttribute("font-size", $"{Convert(fontSize)}pt");
            options?.Apply(text);
            _current.AppendChild(text);
        }

        /// <summary>
        /// Starts a group.
        /// </summary>
        /// <param name="options">The options.</param>
        public void StartGroup(GraphicOptions options = null)
        {
            var elt = _document.CreateElement("g", Namespace);
            options?.Apply(elt);
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
        /// Adds SVG metadata to an existing XML document.
        /// </summary>
        /// <param name="tag">The tag name. The actual tag name will be preceded with 'sc:'</param>
        /// <param name="content">The contents of the tag. This is taken literal, possibly needing CDATA formatting.</param>
        public void AddMetadata(string tag, string content)
        {
            _document.DocumentElement.SetAttribute("xmlns:sc", SimpleCircuitNamespace);

            // Find the metadata tag
            var metadata = _document.DocumentElement.SelectSingleNode("metadata");
            if (metadata == null)
                metadata = _document.DocumentElement.PrependChild(_document.CreateElement("metadata", Namespace));

            // Create the content
            content = content.Replace("]]>", "]]]]><![CDATA["); // Should not happen, but just to be sure
            var elt = _document.CreateElement($"sc:{tag}", SimpleCircuitNamespace);

            // If the text contains data that is multiline, add a newline before and after content for easier reading
            if (content.IndexOfAny(new[] { '\r', '\n' }) >= 0)
                content = Environment.NewLine + content + Environment.NewLine;
            if (content.IndexOfAny(new[] { '"', '\'', '<', '>', '&' }) >= 0)
                elt.AppendChild(_document.CreateCDataSection(content));
            else
                elt.InnerText = content;
            metadata.AppendChild(elt);
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
