using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// The namespace for SimpleCircuit nodes.
        /// </summary>
        public const string SimpleCircuitNamespace = "https://github.com/svenboulanger/SimpleCircuit";

        private readonly XmlDocument _document;
        private XmlNode _current;
        private readonly ExpandableBounds _bounds;
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
        public IElementFormatter ElementFormatter { get; set; }

        /// <summary>
        /// Gets or sets the margin used along the border to make sure everything is included.
        /// </summary>
        public double Margin { get; set; } = 1.0;

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
            _bounds.Expand(new[] { start, end });

            // Create the line
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
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            position = CurrentTransform.Apply(position);
            _bounds.Expand(new[] { position - new Vector2(radius, radius), position + new Vector2(radius, radius) });

            // Make the circle
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
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);

            // Creates the poly
            var poly = _document.CreateElement("polyline", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", string.Join(" ", points.Select(p => $"{Convert(p.X)},{Convert(p.Y)}")));
        }

        /// <summary>
        /// Draws a polygon (a closed shape of straight lines).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The options.</param>
        public void Polygon(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);

            // Create the element
            var poly = _document.CreateElement("polygon", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", string.Join(" ", points.Select(p => $"{Convert(p.X)},{Convert(p.Y)}")));
        }

        /// <summary>
        /// Draws multiple lines. Every two points define a separate line.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The options.</param>
        public void Segments(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);
            
            // Create the path
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);

            int index = 0;
            var d = points.GroupBy(p => (index++) / 2).Select(g =>
            {
                var v = g.ToArray();
                return $"M {Convert(v[0].X)} {Convert(v[0].Y)} L {Convert(v[1].X)} {Convert(v[1].Y)}";
            });
            path.SetAttribute("d", string.Join(" ", d));
        }

        /// <summary>
        /// Draws a smooth bezier curve.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void SmoothBezier(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);

            int index = -1;
            var d = points.GroupBy(p => { index++; return index < 1 ? 0 : (index < 4) ? 1 : 1 + index / 4; }).Select(g =>
            {
                var v = g.ToArray();
                return v.Length switch
                {
                    1 => $"M{Convert(v[0])}",
                    3 => $"C{Convert(v[0])} {Convert(v[1])} {Convert(v[2])}",
                    _ => $"S{Convert(v[0])} {Convert(v[1])} {Convert(v[2])} {Convert(v[3])}",
                };
            });
            path.SetAttribute("d", string.Join(" ", d));
        }

        /// <summary>
        /// Closed bezier curve.
        /// </summary>
        /// <param name="points">The points and handles.</param>
        /// <param name="options">The options.</param>
        public void ClosedBezier(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);

            int index = 2;
            var d = points.GroupBy(p => (index++) / 3).Select(g =>
            {
                var v = g.ToArray();
                return v.Length switch
                {
                    1 => $"M{Convert(v[0])}",
                    _ => $"C{Convert(v[0])}, {Convert(v[1])}, {Convert(v[2])}"
                };
            });
            path.SetAttribute("d", string.Join(" ", d) + "Z");
        }

        /// <summary>
        /// Open bezier curve.
        /// </summary>
        /// <param name="points">The points and handles.</param>
        /// <param name="options">The options.</param>
        public void OpenBezier(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Expand(points);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);

            int index = 2;
            var d = points.GroupBy(p => (index++) / 3).Select(g =>
            {
                var v = g.ToArray();
                return v.Length switch
                {
                    1 => $"M{Convert(v[0])}",
                    _ => $"C{Convert(v[0])}, {Convert(v[1])}, {Convert(v[2])}"
                };
            });
            path.SetAttribute("d", string.Join(" ", d));
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        /// <param name="expand">The direction of the quadrant that the text can expand to.</param>
        /// <param name="options">The options.</param>
        public void Text(string value, Vector2 location, Vector2 expand, GraphicOptions options = null)
        {
            var formatter = ElementFormatter ?? new ElementFormatter();
            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            if (string.IsNullOrWhiteSpace(value))
                return;
            
            // Create the DOM elements and a span element for each 
            var txt = _document.CreateElement("text", Namespace);
            options?.Apply(txt);

            _current.AppendChild(txt);
            List<XmlElement> elements = new();
            foreach (var line in value.Split(new char[] { '\r', '\n', '\\' }))
            {
                var tspan = _document.CreateElement("tspan", Namespace);
                tspan.InnerText = line;
                txt.AppendChild(tspan);
                elements.Add(tspan);
            }

            // Determine the bounds of the lines, which will then determine their position
            var formattedLines = new Bounds[elements.Count];
            double width = 0, height = 0;
            for (int i = 0; i < elements.Count; i++)
            {
                formattedLines[i] = formatter.Format(this, elements[i]);
                width = Math.Max(formattedLines[i].Width, width);
                height += formattedLines[i].Height;

                // Expand the X-direction
                if (expand.X.IsZero())
                {
                    elements[i].SetAttribute("text-anchor", "middle");
                    _bounds.Expand(location - new Vector2(width / 2, 0));
                    _bounds.Expand(location + new Vector2(width / 2, 0));
                }
                else if (expand.X > 0)
                {
                    elements[i].SetAttribute("text-anchor", "start");
                    _bounds.Expand(location);
                    _bounds.Expand(location + new Vector2(width, 0));
                }
                else
                {
                    elements[i].SetAttribute("text-anchor", "end");
                    _bounds.Expand(location - new Vector2(width, 0));
                    _bounds.Expand(location);
                }
            }

            // Draw the text lines with multiple lines
            double y;
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

            for (int i = 0; i < elements.Count; i++)
            {
                y -= formattedLines[i].Top;
                elements[i].SetAttribute("x", Convert(location.X));
                elements[i].SetAttribute("y", Convert(location.Y + y));
                y += formattedLines[i].Bottom;
            }

            txt.SetAttribute("x", Convert(location.X));
            txt.SetAttribute("y", Convert(location.Y));
        }

        /// <summary>
        /// Starts a group.
        /// </summary>
        /// <param name="options">The options.</param>
        public void StartGroup(GraphicOptions options = null)
        {
            var elt = _document.CreateElement("g", Namespace);
            options?.Apply(elt);
            _current.AppendChild(elt);
            _current = elt;
        }

        /// <summary>
        /// Ends the last opened group.
        /// </summary>
        public void EndGroup()
        {
            if (_current.ParentNode != null)
                _current = _current.ParentNode;
        }

        /// <summary>
        /// Gets the SVG xml-document.
        /// </summary>
        /// <returns>The document.</returns>
        public XmlDocument GetDocument()
        {
            var svg = _document.DocumentElement;

            // Try to get the bounds of this
            var bounds = ElementFormatter?.Format(this, svg) ?? _bounds.Bounds;

            // Apply a margin along the edges
            bounds = new Bounds(bounds.Left - Margin, bounds.Top - Margin, bounds.Right + Margin, bounds.Bottom + Margin);
            svg.SetAttribute("width", ((int)bounds.Width * 5).ToString());
            svg.SetAttribute("height", ((int)bounds.Height * 5).ToString());
            svg.SetAttribute("viewBox", $"{Convert(bounds.Left)} {Convert(bounds.Top)} {Convert(bounds.Width + 2 * Margin)} {Convert(bounds.Height + 2 * Margin)}");

            // Add stylesheet info if necessary
            if (!string.IsNullOrWhiteSpace(Style))
            {
                var style = _document.CreateElement("style", Namespace);
                style.InnerText = Style;
                svg.PrependChild(style);
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
        private static string Convert(double value)
        {
            return Math.Round(value, 5).ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a vector to a string for our svg document.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The formatted value.</returns>
        private static string Convert(Vector2 v)
            => $"{Convert(v.X)} {Convert(v.Y)}";
    }
}
