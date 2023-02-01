using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SimpleTexts;
using SimpleCircuit.Parser.SvgPathData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly Stack<ExpandableBounds> _bounds;
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
        /// Gets or sets the line spacing used between text.
        /// </summary>
        public double LineSpacing { get; set; } = 1.0;

        /// <summary>
        /// Removes empty groups.
        /// </summary>
        public bool RemoveEmptyGroups { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag that causes the bounding boxes of groups with an identifier to be rendered as well.
        /// </summary>
        /// <remarks>
        /// Bounding boxes are grouped in a group tag, with the class "bounds".
        /// </remarks>
        public bool RenderBounds { get; set; } = false;

        /// <summary>
        /// Gets or sets the amount to expand the bounds if <see cref="RenderBounds"/> is <c>true</c>.
        /// </summary>
        public double ExpandBounds { get; set; } = 2.0;

        /// <summary>
        /// Creates a new SVG drawing instance.
        /// </summary>
        public SvgDrawing()
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);

            // Make sure we can track the bounds of our vector image
            _bounds = new();
            _bounds.Push(new());
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
                LineSpacing = options.LineSpacing;
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
            // Apply some scale if necessary
            var scale = description.Attributes?["scale"];
            var offset = description.Attributes?["offset"];
            var rotate = description.Attributes?["rotate"];
            if (scale != null || offset != null || rotate != null)
            {
                double s = scale != null ? double.Parse(scale.Value, NumberStyles.Float, CultureInfo.InvariantCulture) : 1.0;
                Vector2 o = new();
                if (offset != null)
                {
                    var lexer = new SvgPathDataLexer(offset.Value);
                    lexer.ParseVector(diagnostics, out o);
                }
                double a = rotate != null ? -double.Parse(rotate.Value, NumberStyles.Float, CultureInfo.InvariantCulture) * Math.PI / 180.0 : 0.0;
                BeginTransform(new Transform(o, Matrix2.Rotate(a) * s));
            }

            DrawXmlActions(description, diagnostics);

            if (scale != null || offset != null || rotate != null)
                EndTransform();
        }
        private void DrawXmlActions(XmlNode parent, IDiagnosticHandler diagnostics)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                // Depending on the node type, let's draw something!
                switch (node.Name)
                {
                    case "line": DrawXmlLine(node, diagnostics); break;
                    case "circle": DrawXmlCircle(node, diagnostics); break;
                    case "path": DrawXmlPath(node, diagnostics); break;
                    case "polygon": DrawXmlPolygon(node, diagnostics); break;
                    case "polyline": DrawXmlPolyline(node, diagnostics); break;
                    case "rect": DrawXmlRectangle(node, diagnostics); break;
                    case "text": DrawXmlText(node, diagnostics); break;
                    case "group":
                    case "g":
                        // Parse options
                        var options = ParsePathOptions(node);
                        StartGroup(options);
                        DrawXmlActions(node, diagnostics);
                        EndGroup();
                        break;
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
            success &= node.ParseVector("x1", "y1", diagnostics, out var start);
            success &= node.ParseVector("x2", "y2", diagnostics, out var end);
            if (!success)
                return;
            var options = ParsePathOptions(node);

            // Draw the line
            Line(start, end, options);
        }
        private void DrawXmlPolygon(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            List<Vector2> points;
            string pointdata = node.Attributes["points"]?.Value;
            if (pointdata != null)
            {
                var lexer = new SvgPathDataLexer(pointdata);
                points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
            }
            else if (node.HasChildNodes)
            {
                points = new(node.ChildNodes.Count);
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "p")
                    {
                        if (!child.ParseVector("x", "y", diagnostics, out var result))
                            continue;
                        points.Add(result);
                    }
                }
            }
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW002",
                    "No polygon data given"));
                return;
            }

            // Draw the polygon
            Polygon(points, options);
        }
        private void DrawXmlPolyline(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            string pointdata = node.Attributes["points"]?.Value;
            List<Vector2> points;
            if (pointdata != null)
            {
                var lexer = new SvgPathDataLexer(pointdata);
                points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
            }
            else if (node.HasChildNodes)
            {
                points = new(node.ChildNodes.Count);
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "p" || child.Name == "point")
                    {
                        if (!child.ParseVector("x", "y", diagnostics, out var result))
                            continue;
                        points.Add(result);
                    }
                }
            }
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DRAW002",
                    "No polyline data given"));
                return;
            }

            // Draw the polyline
            Polyline(points, options);
        }
        private void DrawXmlCircle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            // Get the coordinates
            bool success = true;
            success &= node.ParseVector("cx", "cy", diagnostics, out Vector2 center);
            success &= node.ParseCoordinate("r", diagnostics, out double r);
            if (!success)
                return;
            var options = ParsePathOptions(node);

            // Draw the circle
            Circle(center, r, options);
        }
        private void DrawXmlPath(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            string d = node.Attributes["d"]?.Value;
            if (!string.IsNullOrWhiteSpace(d))
            {
                Path(b =>
                {
                    var lexer = new SvgPathDataLexer(d);
                    SvgPathDataParser.Parse(lexer, b, diagnostics);
                }, options);
            }
            else if (node.HasChildNodes)
            {
                Path(b =>
                {
                    foreach (XmlNode cmd in node.ChildNodes)
                    {
                        string action = cmd.Name;
                        bool result = true;
                        Vector2 h1, h2, p;
                        double d;
                        switch (action)
                        {
                            case "M":
                                if (!cmd.ParseVector("x", "y", diagnostics, out p))
                                    continue;
                                b.MoveTo(p);
                                break;
                            case "m":
                                if (!cmd.TryParseVector("x", "y", diagnostics, new(), out p) &&
                                    !cmd.ParseVector("dx", "dy", diagnostics, out p))
                                    continue;
                                b.Move(p);
                                break;
                            case "L":
                                if (!cmd.ParseVector("x", "y", diagnostics, out p))
                                    continue;
                                b.LineTo(p);
                                break;
                            case "l":
                                if (!cmd.TryParseVector("x", "y", diagnostics, new(), out p) &&
                                    !cmd.ParseVector("dx", "dy", diagnostics, out p))
                                    continue;
                                b.Line(p);
                                break;
                            case "H":
                                if (!cmd.ParseCoordinate("x", diagnostics, out d))
                                    continue;
                                b.HorizontalTo(d);
                                break;
                            case "h":
                                if (!cmd.ParseCoordinate("dx", diagnostics, out d))
                                    continue;
                                b.Horizontal(d);
                                break;
                            case "V":
                                if (!cmd.ParseCoordinate("y", diagnostics, out d))
                                    continue;
                                b.VerticalTo(d);
                                break;
                            case "v":
                                if (!cmd.ParseCoordinate("dy", diagnostics, out d))
                                    continue;
                                b.Vertical(d);
                                break;
                            case "C":
                                result &= cmd.ParseVector("x1", "y1", diagnostics, out h1);
                                result &= cmd.ParseVector("x2", "y2", diagnostics, out h2);
                                result &= cmd.ParseVector("x", "y", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.CurveTo(h1, h2, p);
                                break;
                            case "c":
                                result &= cmd.ParseVector("dx1", "dy1", diagnostics, out h1);
                                result &= cmd.ParseVector("dx2", "dy2", diagnostics, out h2);
                                result &= cmd.ParseVector("dx", "dy", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.Curve(h1, h2, p);
                                break;
                            case "S":
                                result &= cmd.ParseVector("x2", "y2", diagnostics, out h2);
                                result &= cmd.ParseVector("x", "y", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.SmoothTo(h2, p);
                                break;
                            case "s":
                                result &= cmd.ParseVector("dx2", "dy2", diagnostics, out h2);
                                result &= cmd.ParseVector("x2", "y2", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.Smooth(h2, p);
                                break;
                            case "Q":
                                result &= cmd.ParseVector("x1", "y1", diagnostics, out h1);
                                result &= cmd.ParseVector("x", "y", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.QuadCurveTo(h1, p);
                                break;
                            case "q":
                                result &= cmd.ParseVector("dx1", "dy1", diagnostics, out h1);
                                result &= cmd.ParseVector("x", "y", diagnostics, out p);
                                if (!result)
                                    continue;
                                b.QuadCurve(h1, p);
                                break;
                            case "T":
                                if (!cmd.ParseVector("x", "y", diagnostics, out p))
                                    continue;
                                b.SmoothQuadTo(p);
                                break;
                            case "t":
                                if (!cmd.ParseVector("dx", "dy", diagnostics, out p))
                                    continue;
                                b.SmoothQuad(p);
                                break;
                            case "z":
                            case "Z":
                                b.Close();
                                break;

                            default:
                                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "DRAW001",
                                    $"Could not recognize path command '{cmd}'."));
                                break;
                        }
                    }
                }, options);
            }
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "DW001",
                    "Could not find path data"));
            }
        }
        private void DrawXmlRectangle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            if (!node.ParseVector("x", "y", diagnostics, out var location))
                return;
            if (!node.ParseVector("width", "height", diagnostics, out var size))
                return;
            node.TryParseCoordinate("rx", diagnostics, double.NaN, out double rx);
            node.TryParseCoordinate("ry", diagnostics, double.NaN, out double ry);
            if (double.IsNaN(rx) && !double.IsNaN(ry))
                rx = ry;
            else if (double.IsNaN(ry) && !double.IsNaN(rx))
                ry = rx;
            else if (double.IsNaN(rx) && double.IsNaN(ry))
            {
                rx = 0.0;
                ry = 0.0;
            }

            // Draw the rectangle
            double kx = 0.55191502449351057 * rx;
            double ky = 0.55191502449351057 * ry;
            Path(b =>
            {
                b.MoveTo(location + new Vector2(rx, 0));
                b.Horizontal(size.X - 2 * rx);
                if (rx != 0.0)
                    b.Curve(new(kx, 0), new(rx, ry - ky), new(rx, ry));
                b.Vertical(size.Y - 2 * ry);
                if (ry != 0.0)
                    b.Curve(new(0, ky), new(-rx + kx, ry), new(-rx, ry));
                b.Horizontal(2 * rx - size.X);
                if (rx != 0.0)
                    b.Curve(new(-kx, 0), new(-rx, ky - ry), new(-rx, -ry));
                b.Vertical(2 * ry - size.Y);
                if (ry != 0)
                    b.Curve(new(0, -ky), new(rx - kx, -ry), new(rx, -ry));
                b.Close();
            }, options);
        }
        private void DrawXmlText(XmlNode node, IDiagnosticHandler diagnostics)
        {
            var options = ParsePathOptions(node);
            string label = node.Attributes["value"]?.Value;
            if (string.IsNullOrWhiteSpace(label))
                return;

            // Get the coordinates
            if (!node.ParseVector("x", "y", diagnostics, out Vector2 location))
                return;
            node.TryParseVector("nx", "ny", diagnostics, new(), out Vector2 expand);
            Text(label, location, expand, options);
        }
        private PathOptions ParsePathOptions(XmlNode parent)
        {
            if (parent == null)
                return null;
            if (parent.Attributes == null)
                return null;
            var options = new PathOptions();

            // Read some styling
            var attribute = parent.Attributes?["style"];
            if (attribute != null)
                options.Style = attribute.Value;

            // Read the class
            attribute = parent.Attributes?["class"];
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
            _bounds.Peek().Expand(new[] { start, end });

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
            _bounds.Peek().Expand(new[] { position - new Vector2(radius, radius), position + new Vector2(radius, radius) });

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
        /// <param name="options">The path options.</param>
        public void Polyline(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Peek().Expand(points);

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
            _bounds.Peek().Expand(points);

            // Create the element
            var poly = _document.CreateElement("polygon", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", string.Join(" ", points.Select(p => $"{Convert(p.X)},{Convert(p.Y)}")));
        }

        /// <summary>
        /// Draws a smooth bezier curve.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="classes">The classes.</param>
        public void SmoothBezier(IEnumerable<Vector2> points, PathOptions options = null)
        {
            points = CurrentTransform.Apply(points);
            _bounds.Peek().Expand(points);

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
            _bounds.Peek().Expand(points);

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
            _bounds.Peek().Expand(points);

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
        public void Text(string value, Vector2 location, Vector2 expand, GraphicOptions options = null, bool transformText = true)
        {
            // Don't bother with text that is null or just whitespaces.
            if (string.IsNullOrWhiteSpace(value))
                return;

            var formatter = ElementFormatter ?? new ElementFormatter();
            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            // Create the DOM elements and a span element for each 
            var txt = _document.CreateElement("text", Namespace);
            options?.Apply(txt);
            _current.AppendChild(txt);

            // Apply text
            IEnumerable<string> lines;
            if (transformText)
            {
                var lexer = new SimpleTextLexer(value);
                var context = new SimpleTextContext();
                SimpleTextParser.Parse(lexer, context);
                lines = context.Lines;
            }
            else
            {
                lines = value.Split('\n');
            }
            List<XmlElement> elements = new();
            foreach (var line in lines)
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
                if (i > 0)
                    height += LineSpacing;
                height += formattedLines[i].Height;

                // Expand the X-direction
                if (expand.X.IsZero())
                {
                    elements[i].SetAttribute("text-anchor", "middle");
                    _bounds.Peek().Expand(location - new Vector2(width / 2, 0));
                    _bounds.Peek().Expand(location + new Vector2(width / 2, 0));
                }
                else if (expand.X > 0)
                {
                    elements[i].SetAttribute("text-anchor", "start");
                    _bounds.Peek().Expand(location);
                    _bounds.Peek().Expand(location + new Vector2(width, 0));
                }
                else
                {
                    elements[i].SetAttribute("text-anchor", "end");
                    _bounds.Peek().Expand(location - new Vector2(width, 0));
                    _bounds.Peek().Expand(location);
                }
            }

            // Draw the text lines with multiple lines
            double y;
            if (expand.Y.IsZero())
            {
                _bounds.Peek().Expand(location - new Vector2(0, height / 2));
                _bounds.Peek().Expand(location + new Vector2(0, height / 2));
                y = -height / 2;
            }
            else if (expand.Y > 0)
            {
                _bounds.Peek().Expand(location);
                _bounds.Peek().Expand(location + new Vector2(0, height));
                y = 0.0;
            }
            else
            {
                _bounds.Peek().Expand(location - new Vector2(0, height));
                _bounds.Peek().Expand(location);
                y = -height;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                y -= formattedLines[i].Top;
                elements[i].SetAttribute("x", Convert(location.X));
                elements[i].SetAttribute("y", Convert(location.Y + y));
                y += formattedLines[i].Bottom + LineSpacing;
            }

            txt.SetAttribute("x", Convert(location.X));
            txt.SetAttribute("y", Convert(location.Y));
        }

        /// <summary>
        /// Draws a path.
        /// </summary>
        /// <param name="action">The actions.</param>
        /// <param name="options">The path options.</param>
        public void Path(Action<PathBuilder> action, PathOptions options = null)
        {
            if (action == null)
                return;
            var builder = new PathBuilder(_bounds.Peek(), CurrentTransform);
            action(builder);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", builder.ToString());

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

            // Track the bounds of the group
            _bounds.Push(new());
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

            // Go to the parent bounds
            if (_bounds.Count > 1)
            {
                var bounds = _bounds.Pop();
                var cBounds = _bounds.Peek();
                cBounds.Expand(bounds);

                // Draw a rectangle on top of the group to show
                if (RenderBounds)
                {
                    string id = group.Attributes["id"]?.Value;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var b = bounds.Bounds;
                        var center = new Vector2(0.5 * (b.Left + b.Right), 0.5 * (b.Top + b.Bottom));
                        double left = b.Left - ExpandBounds, right = b.Right + ExpandBounds;
                        double top = b.Top - ExpandBounds, bottom = b.Bottom + ExpandBounds;

                        StartGroup(new("bounds"));
                        Polygon(new Vector2[]
                        {
                            new(left, top), new(right, top),
                            new(right, bottom), new(left, bottom)
                        });

                        // Show the ID in the middle of the box
                        Text(id, center, new(), transformText: false);
                        EndGroup();
                    }
                }
            }
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
            var bounds = ElementFormatter?.Format(this, svg) ?? _bounds.Peek().Bounds;

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
            metadata ??= _document.DocumentElement.PrependChild(_document.CreateElement("metadata", Namespace));

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
