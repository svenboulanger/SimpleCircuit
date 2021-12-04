using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


        private readonly static CultureInfo _culture = CultureInfo.InvariantCulture;
        private readonly XmlDocument _document;
        private XmlNode _current;
        private readonly ExpandableBounds _bounds;
        private readonly Stack<Transform> _tf = new();
        private static Regex _superSubscriptRegex = new(@"[\^_](\{(?<content>[^""\}]+)\}|(?<content>\w+))", RegexOptions.Compiled);

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
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets the text formatter.
        /// </summary>
        public IElementFormatter ElementFormatter { get; set; }

        /// <summary>
        /// Gets or sets the margin used along the border to make sure everything is included.
        /// </summary>
        public double Margin { get; set; } = 1.0;

        /// <summary>
        /// Removes empty groups.
        /// </summary>
        public bool RemoveEmptyGroups { get; set; } = true;

        /// <summary>
        /// Creates a new SVG drawing instance.
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
        /// Creates a new SVG drawing instance.
        /// </summary>
        /// <param name="options">The options.</param>
        public SvgDrawing(Options options)
            : this()
        {
            if (options != null)
            {
                Margin = options.Margin;
                RemoveEmptyGroups = options.RemoveEmptyGroups;
            }
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

        /// <inheritdoc />
        public void DrawXml(XmlNode description, IDiagnosticHandler diagnostics)
        {
            foreach (XmlNode node in description.ChildNodes)
            {
                // Depending on the node type, let's draw something!
                switch (node.Name)
                {
                    case "line": DrawXmlLine(node, diagnostics); break;
                    case "circle": DrawXmlCircle(node, diagnostics); break;
                    case "path": DrawXmlPath(node, diagnostics); break;
                    case "polygon": DrawXmlPolygon(node, diagnostics); break;
                    case "polyline": DrawXmlPolyline(node, diagnostics); break;
                    case "text": DrawXmlText(node, diagnostics); break;
                    default:
                        diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001",
                            $"Could not recognize drawing node '{node.Name}'"));
                        break;
                }
            }
        }
        private void DrawXmlLine(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= ParseVector(node, "x1", "y1", diagnostics, out var start);
            success &= ParseVector(node, "x2", "y2", diagnostics, out var end);
            if (!success)
                return;
            var options = ParsePathOptions(node);

            // Draw the line
            Line(start, end, options);
        }
        private void DrawXmlPolygon(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.ChildNodes.Count == 0)
                return;

            List<Vector2> points = new(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "p")
                {
                    if (!ParseVector(child, "x", "y", diagnostics, out var result))
                        continue;
                    points.Add(result);
                }
            }
            var options = ParsePathOptions(node);

            // Draw the polygon
            Polygon(points, options);
        }
        private void DrawXmlPolyline(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.ChildNodes.Count == 0)
                return;

            List<Vector2> points = new(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "p")
                {
                    if (!ParseVector(child, "x", "y", diagnostics, out var result))
                        continue;
                    points.Add(result);
                }
            }
            var options = ParsePathOptions(node);

            // Draw the polyline
            Polyline(points, options);
        }
        private void DrawXmlCircle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            // Get the coordinates
            bool success = true;
            success &= ParseVector(node, "cx", "cy", diagnostics, out Vector2 center);
            success &= ParseCoordinate(node, "r", diagnostics, out double r);
            if (!success)
                return;
            var options = ParsePathOptions(node);

            // Draw the circle
            Circle(center, r, options);
        }
        private void DrawXmlPath(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);

            // Add the path
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);

            // Create the data
            StringBuilder sb = new();
            Vector2 pathStart = new(); // Keeps track of the start of a path
            Vector2 current = new(); // Keeps track of the current location
            Vector2 h1 = new(), h2 = new(), p = new(); // Keeps track of handles and end points
            bool success;
            foreach (XmlNode cmd in node.ChildNodes)
            {
                success = true;
                string action = cmd.Name;
                switch (action)
                {
                    // Move absolute value
                    case "M":
                        if (!ParseVector(cmd, "x", "y", diagnostics, out p))
                            continue;
                        p = CurrentTransform.Apply(p);
                        sb.Append($" M{Convert(p.X)} {Convert(p.Y)}");
                        h1 = h2 = pathStart = current = p;
                        _bounds.Expand(current);
                        break;

                    // Move relative distance
                    case "m":
                        if (!ParseVector(cmd, "x", "y", diagnostics, out p))
                            continue;
                        p = CurrentTransform.Apply(p);
                        sb.Append($" m{Convert(p.X)} {Convert(p.Y)}");
                        current += p;
                        h1 = h2 = pathStart = current;
                        _bounds.Expand(current);
                        break;

                    // Line to absolute position
                    case "L":
                        if (!ParseVector(cmd, "x", "y", diagnostics, out p))
                            continue;
                        p = CurrentTransform.Apply(p);
                        sb.Append($" L{Convert(p.X)} {Convert(p.Y)}");
                        h1 = h2 = current = p;
                        _bounds.Expand(current);
                        break;

                    // Line to relative position
                    case "l":
                        if (!ParseVector(cmd, "x", "y", diagnostics, out p))
                            continue;
                        p = CurrentTransform.ApplyDirection(p);
                        sb.Append($" l{Convert(p.X)} {Convert(p.Y)}");
                        current += p;
                        h1 = h2 = current;
                        _bounds.Expand(current);
                        break;

                    case "C":
                        success &= ParseVector(cmd, "x1", "y1", diagnostics, out h1);
                        success &= ParseVector(cmd, "x2", "y2", diagnostics, out h2);
                        success &= ParseVector(cmd, "x", "y", diagnostics, out p);
                        if (!success)
                            continue;
                        h1 = CurrentTransform.Apply(h1);
                        h2 = CurrentTransform.Apply(h2);
                        p = CurrentTransform.Apply(p);
                        _bounds.Expand(new[] { h1, h2, p });
                        sb.Append($" C{Convert(h1.X)} {Convert(h1.Y)} {Convert(h2.X)} {Convert(h2.Y)} {Convert(p.X)} {Convert(p.Y)}");
                        current = p;
                        break;

                    case "c":
                        success &= ParseVector(cmd, "dx1", "dy1", diagnostics, out h1);
                        success &= ParseVector(cmd, "dx2", "dy2", diagnostics, out h2);
                        success &= ParseVector(cmd, "dx", "dy", diagnostics, out p);
                        if (!success)
                            continue;
                        h1 = CurrentTransform.ApplyDirection(h1);
                        h2 = CurrentTransform.ApplyDirection(h2);
                        p = CurrentTransform.ApplyDirection(p);
                        _bounds.Expand(new[] { h1, h2, p }.Select(v => v + p));
                        sb.Append($" c{Convert(h1.X)} {Convert(h1.Y)} {Convert(h2.X)} {Convert(h2.Y)} {Convert(p.X)} {Convert(p.Y)}");
                        current += p;
                        break;

                    case "S":
                        h1 = h2; // We should use the last handle position
                        success &= ParseVector(cmd, "x2", "y2", diagnostics, out h2);
                        success &= ParseVector(cmd, "x", "y", diagnostics, out p);
                        if (!success)
                            continue;
                        h2 = CurrentTransform.Apply(h2);
                        p = CurrentTransform.Apply(p);
                        _bounds.Expand(new[] { 2 * current - h1, h2, p });
                        sb.Append($" S{Convert(h2.X)} {Convert(h2.Y)} {Convert(p.X)} {Convert(p.Y)}");
                        current = p;
                        break;

                    case "s":
                        h1 = h2;
                        success &= ParseVector(cmd, "dx2", "dy2", diagnostics, out h2);
                        success &= ParseVector(cmd, "dx", "dy", diagnostics, out p);
                        if (!success)
                            continue;
                        h2 = CurrentTransform.ApplyDirection(h2);
                        p = CurrentTransform.ApplyDirection(p);
                        _bounds.Expand(new[] { 2 * current - h1, h2 + current, p + current });
                        sb.Append($" s{Convert(h2.X)} {Convert(h2.Y)} {Convert(p.X)} {Convert(p.Y)}");
                        current += p;
                        break;

                    case "z":
                    case "Z":
                        sb.Append(" Z");
                        current = pathStart;
                        break;
                }
            }
            path.SetAttribute("d", sb.ToString());
        }
        private void DrawXmlText(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            string label = node.Attributes["value"]?.Value;
            if (string.IsNullOrWhiteSpace(label))
                return;

            // Get the coordinates
            if (!ParseVector(node, "x", "y", diagnostics, out Vector2 location))
                return;
            TryParseVector(node, "nx", "ny", diagnostics, new(), out Vector2 expand);
            Text(label, location, expand, options);
        }
        private bool ParseCoordinate(XmlNode node, string attributeName, IDiagnosticHandler diagnostics, out double result)
        {
            string value = node.Attributes?[attributeName]?.Value;
            if (value == null)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected attribute '{attributeName}' on {node.Name}."));
                result = 0.0;
                return false;
            }
            if (!double.TryParse(value, NumberStyles.Float, _culture, out result))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected coordinate for '{attributeName}' on {node.Name}, but was '{value}'."));
                result = 0.0;
                return false;
            }
            return true;
        }
        private bool ParseVector(XmlNode node, string xAttribute, string yAttribute, IDiagnosticHandler diagnostics, out Vector2 result)
        {
            bool success = true;
            success &= ParseCoordinate(node, xAttribute, diagnostics, out double x);
            success &= ParseCoordinate(node, yAttribute, diagnostics, out double y);
            result = new(x, y);
            return success;
        }
        private bool TryParseCoordinate(XmlNode node, string attributeName, IDiagnosticHandler diagnostics, double defaultValue, out double result)
        {
            string value = node.Attributes?[attributeName]?.Value;
            if (value == null)
            {
                result = defaultValue;
                return false;
            }
            if (!double.TryParse(value, NumberStyles.Float, _culture, out result))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW001", $"Expected coordinate for '{attributeName}' on {node.Name}, but was '{value}'."));
                result = defaultValue;
                return false;
            }
            return true;
        }
        private bool TryParseVector(XmlNode node, string xAttribute, string yAttribute, IDiagnosticHandler diagnostics, Vector2 defaultValue, out Vector2 result)
        {
            bool success = true;
            success &= TryParseCoordinate(node, xAttribute, diagnostics, defaultValue.X, out double x);
            success &= TryParseCoordinate(node, yAttribute, diagnostics, defaultValue.Y, out double y);
            result = new(x, y);
            return success;
        }
        private PathOptions ParsePathOptions(XmlNode parent)
        {
            if (parent == null)
                return null;
            if (parent.Attributes == null)
                return null;

            var options = new PathOptions();

            // Read the class
            var attribute = parent.Attributes?["class"];
            if (attribute != null)
            {
                foreach (string name in attribute.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    options.Classes.Add(name);
            }

            // Read a marker
            attribute = parent.Attributes?["end"];
            if (attribute != null)
            {
                switch (attribute.Value.ToLower())
                {
                    case "arrow": options.EndMarker = PathOptions.MarkerTypes.Arrow; break;
                    case "slash": options.EndMarker = PathOptions.MarkerTypes.Slash; break;
                    case "dot": options.EndMarker = PathOptions.MarkerTypes.Dot; break;
                }
            }
            attribute = parent.Attributes?["middle"];
            if (attribute != null)
            {
                switch (attribute.Value.ToLower())
                {
                    case "arrow": options.MiddleMarker = PathOptions.MarkerTypes.Arrow; break;
                    case "slash": options.MiddleMarker = PathOptions.MarkerTypes.Slash; break;
                    case "dot": options.MiddleMarker = PathOptions.MarkerTypes.Dot; break;
                }
            }
            attribute = parent.Attributes?["start"];
            if (attribute != null)
            {
                switch (attribute.Value.ToLower())
                {
                    case "arrow": options.StartMarker = PathOptions.MarkerTypes.Arrow; break;
                    case "slash": options.StartMarker = PathOptions.MarkerTypes.Slash; break;
                    case "dot": options.StartMarker = PathOptions.MarkerTypes.Dot; break;
                }
            }
            return options;
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
        /// Draw an arc clockwise from the starting point to the ending point with a given radius.
        /// </summary>
        /// <param name="center">The center point of the arc.</param>
        /// <param name="startAngle">The starting angle of the arc.</param>
        /// <param name="endAngle">The end angle of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="options">The path options.</param>
        /// <param name="intermediatePoints">The number of intermediate points.</param>
        public void Arc(Vector2 center, double startAngle, double endAngle, double radius, PathOptions options = null, int intermediatePoints = 0)
        {
            if (endAngle < startAngle)
                endAngle += 2 * Math.PI;

            var da = (endAngle - startAngle) / (intermediatePoints + 1);
            var hl = 4.0 / 3.0 * Math.Tan(da * 0.25) * radius;

            var pts = new List<Vector2>((intermediatePoints + 1) * 4);
            Vector2 p = Vector2.Normal(startAngle) * radius + center;
            pts.Add(p);
            double alpha = startAngle;
            for (int i = 0; i <= intermediatePoints; i++)
            {
                pts.Add(p + Vector2.Normal(alpha + Math.PI / 2) * hl);
                alpha += da;
                p = center + Vector2.Normal(alpha) * radius;
                pts.Add(p - Vector2.Normal(alpha + Math.PI / 2) * hl);
                pts.Add(p);
            }
            OpenBezier(pts, options);
        }

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="rx">The radius along the horizontal axis.</param>
        /// <param name="ry">The radius along the vertical axis.</param>
        /// <param name="options">The options.</param>
        public void Ellipse(Vector2 center, double rx, double ry, PathOptions options = null)
        {
            double kx = rx * 0.552284749831;
            double ky = ry * 0.552284749831;
            ClosedBezier(new Vector2[]
            {
                new(-rx, 0),
                new(-rx, -ky), new(-kx, -ry), new(0, -ry),
                new(kx, -ry), new(rx, -ky), new(rx, 0),
                new(rx, ky), new(kx, ry), new(0, ry),
                new(-kx, ry), new(0, ky), new(-rx, 0)
            }.Select(v => v + center), options);
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

            // Apply text
            value = TransformText(value);
            List<XmlElement> elements = new();
            foreach (var line in value.Split(new char[] { '\r', '\n', '\\' }))
            {
                var tspan = _document.CreateElement("tspan", Namespace);
                PopulateText(tspan, line);
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

        private string TransformText(string value)
        {
            value = value.Replace("\\\"", "\"");
            value = _superSubscriptRegex.Replace(value, match =>
            {
                if (match.Value[0] == '^')
                    return $"<tspan class=\"super\" dy=\"-0.5em\">{match.Groups["content"].Value}</tspan>";
                else
                    return $"<tspan class=\"sub\" dy=\"0.5em\">{match.Groups["content"].Value}</tspan>";
            });
            return value;
        }
        private void PopulateText(XmlNode element, string line)
        {
            var fragment = _document.CreateDocumentFragment();
            fragment.InnerXml = $"<root xmlns=\"{Namespace}\">{line}</root>";

            var nodes = fragment.ChildNodes[0].ChildNodes;
            while (nodes.Count > 0)
                element.AppendChild(nodes[0]);
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
            var group = _current;
            var parent = _current.ParentNode;

            if (RemoveEmptyGroups && group.ChildNodes.Count == 0)
                parent.RemoveChild(group);

            if (parent != null)
                _current = parent;
        }

        /// <summary>
        /// Gets the SVG xml-document.
        /// </summary>
        /// <returns>The document.</returns>
        public XmlDocument GetDocument()
        {
            var svg = _document.DocumentElement;

            // Add stylesheet info if necessary
            if (!string.IsNullOrWhiteSpace(Style))
            {
                var style = _document.CreateElement("style", Namespace);
                style.InnerText = Style;
                svg.PrependChild(style);
            }

            // Try to get the bounds of this
            var bounds = ElementFormatter?.Format(this, svg) ?? _bounds.Bounds;

            // Apply a margin along the edges
            bounds = new Bounds(bounds.Left - Margin, bounds.Top - Margin, bounds.Right + Margin, bounds.Bottom + Margin);
            svg.SetAttribute("width", ((int)bounds.Width * 5).ToString());
            svg.SetAttribute("height", ((int)bounds.Height * 5).ToString());
            svg.SetAttribute("viewBox", $"{Convert(bounds.Left)} {Convert(bounds.Top)} {Convert(bounds.Width)} {Convert(bounds.Height)}");

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
