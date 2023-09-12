using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.SvgPathData;
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
        private readonly Stack<ExpandableBounds> _bounds;
        private readonly Stack<Transform> _tf = new();

        /// <summary>
        /// Gets the current transform.
        /// </summary>
        /// <value>
        /// The current transform.
        /// </value>
        public Transform CurrentTransform => _tf.Peek();

        /// <summary>
        /// Gets or sets the style of the drawing.
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Gets the formatter.
        /// </summary>
        public ITextFormatter Formatter { get; }

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
        public double ExpandBounds { get; set; } = 0.0;

        /// <summary>
        /// Gets the diagnostic handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Creates a new SVG drawing instance.
        /// </summary>
        public SvgDrawing(ITextFormatter formatter = null, IDiagnosticHandler diagnostics = null)
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);

            // Make sure we can track the bounds of our vector image
            _bounds = new();
            _bounds.Push(new());

            // Create a formatter
            Formatter = formatter ?? new ElementFormatter();
            Diagnostics = diagnostics;
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
        /// Ends the last transform.
        /// </summary>
        public void EndTransform()
        {
            _tf.Pop();
            if (_tf.Count == 0)
                _tf.Push(Transform.Identity);
        }

        /// <inheritdoc />
        public void DrawXml(XmlNode description, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            // Apply some scale if necessary
            double scale = 1.0, rotate = 0.0;
            Vector2 offset = new();
            bool transform = false;
            foreach (XmlAttribute attribute in description.Attributes)
            {
                switch (attribute.Name)
                {
                    case "scale": attribute.ParseScalar(diagnostics, out scale, ErrorCodes.InvalidXmlScale); transform = true; break;
                    case "offset":
                        var lexer = new SvgPathDataLexer(attribute.Value.AsMemory());
                        lexer.ParseVector(diagnostics, out offset);
                        transform = true;
                        break;
                    case "rotate": attribute.ParseScalar(diagnostics, out rotate, ErrorCodes.InvalidXmlRotation); transform = true; break;
                    default:
                        diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                        break;
                }
            }

            if (transform)
                BeginTransform(new Transform(offset, Matrix2.Rotate(rotate) * scale));
            DrawXmlActions(description, context, diagnostics);
            if (transform)
                EndTransform();
        }
        private void DrawXmlActions(XmlNode parent, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                // Depending on the node type, let's draw something!
                switch (node.Name)
                {
                    case "#comment": break;
                    case "line": DrawXmlLine(node, diagnostics); break;
                    case "circle": DrawXmlCircle(node, diagnostics); break;
                    case "path": DrawXmlPath(node, diagnostics); break;
                    case "polygon": DrawXmlPolygon(node, diagnostics); break;
                    case "polyline": DrawXmlPolyline(node, diagnostics); break;
                    case "rect": DrawXmlRectangle(node, diagnostics); break;
                    case "text": DrawXmlText(node, context, diagnostics); break;
                    case "group":
                    case "g":
                        // Parse options
                        GraphicOptions options = new();
                        if (node.Attributes != null)
                        {
                            bool success = true;
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                if (!ParseGraphicOption(options, attribute))
                                {
                                    diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name, node.Name);
                                    success = false;
                                }
                            }
                            if (!success)
                                return;
                        }
                        BeginGroup(options);
                        DrawXmlActions(node, context,diagnostics);
                        EndGroup();
                        break;
                    default:
                        diagnostics?.Post(ErrorCodes.CouldNotRecognizeDrawingCommand, node.Name);
                        break;
                }
            }
        }
        private void DrawXmlLine(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            bool success = true;
            GraphicOptions options = new();
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "x1": success &= attribute.ParseScalar(diagnostics, out x1); break;
                    case "y1": success &= attribute.ParseScalar(diagnostics, out y1); break;
                    case "x2": success &= attribute.ParseScalar(diagnostics, out x2); break;
                    case "y2": success &= attribute.ParseScalar(diagnostics, out y2); break;
                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            // Draw the line
            Line(new(x1, y1), new(x2, y2), options);
        }
        private void DrawXmlPolygon(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            List<Vector2> points = null;
            bool success = true;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "points":
                        var lexer = new SvgPathDataLexer(attribute.Value.AsMemory());
                        points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
                        break;

                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            // Draw the polygon
            Polygon(points, options);
        }
        private void DrawXmlPolyline(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            List<Vector2> points = null;
            bool success = true;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "points":
                        var lexer = new SvgPathDataLexer(attribute.Value.AsMemory());
                        points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
                        break;

                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            // Draw the polyline
            Polyline(points, options);
        }
        private void DrawXmlCircle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            bool success = true;
            double cx = 0, cy = 0, r = 0;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "cx": success &= attribute.ParseScalar(diagnostics, out cx); break;
                    case "cy": success &= attribute.ParseScalar(diagnostics, out cy); break;
                    case "r": success &= attribute.ParseScalar(diagnostics, out r); break;
                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Value);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            // Draw the circle
            Circle(new(cx, cy), r, options);
        }
        private void DrawXmlPath(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            string pathData = null;
            bool success = true;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "d": pathData = attribute.Value; break;
                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name, node.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            if (!string.IsNullOrWhiteSpace(pathData))
            {
                Path(b =>
                {
                    var lexer = new SvgPathDataLexer(pathData.AsMemory());
                    SvgPathDataParser.Parse(lexer, b, diagnostics);
                }, options);
            }
        }
        private void DrawXmlRectangle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            bool success = true;
            double x = 0, y = 0, width = 0, height = 0, rx = double.NaN, ry = double.NaN;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "x": success &= attribute.ParseScalar(diagnostics, out x); break;
                    case "y": success &= attribute.ParseScalar(diagnostics, out y); break;
                    case "width": success &= attribute.ParseScalar(diagnostics, out width); break;
                    case "height": success &= attribute.ParseScalar(diagnostics, out height); break;
                    case "rx": success &= attribute.ParseScalar(diagnostics, out rx); break;
                    case "ry": success &= attribute.ParseScalar(diagnostics, out ry); break;
                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name, node.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;

            // Draw the rectangle
            this.Rectangle(x, y, x + width, y + height, rx, ry, options);
        }
        private void DrawXmlText(XmlNode node, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            GraphicOptions options = new();
            bool success = true;
            double x = 0, y = 0, nx = 0, ny = 0;
            string value = null;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                switch (attribute.Name)
                {
                    case "x": success &= attribute.ParseScalar(diagnostics, out x); break;
                    case "y": success &= attribute.ParseScalar(diagnostics, out y); break;
                    case "nx": success &= attribute.ParseScalar(diagnostics, out nx); break;
                    case "ny": success &= attribute.ParseScalar(diagnostics, out ny); break;
                    case "value": value = attribute.Value; break;
                    default:
                        if (!ParseGraphicOption(options, attribute))
                        {
                            diagnostics?.Post(ErrorCodes.UnrecognizedXmlAttribute, attribute.Name, node.Name);
                            success = false;
                        }
                        break;
                }
            }
            if (!success)
                return;
            if (value != null && context != null)
                value = context.TransformText(value);
            Text(value, new Vector2(x, y), new Vector2(nx, ny), options);
        }
        private bool ParseGraphicOption(GraphicOptions options, XmlAttribute attribute)
        {
            switch (attribute.Name)
            {
                case "style":
                    options.Style = attribute.Value;
                    break;

                case "class":
                    foreach (string name in attribute.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        options.Classes.Add(name);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Expands the drawing to include the specified point.
        /// </summary>
        /// <param name="point">The point to expand.</param>
        public void Expand(Vector2 point)
            => _bounds.Peek().Expand(CurrentTransform.Apply(point));

        /// <inheritdoc />
        public Bounds Line(Vector2 start, Vector2 end, GraphicOptions options = null)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);
            var bounds = new Bounds(start, end);

            // Create the line
            var line = _document.CreateElement("line", Namespace);
            line.SetAttribute("x1", Convert(start.X));
            line.SetAttribute("y1", Convert(start.Y));
            line.SetAttribute("x2", Convert(end.X));
            line.SetAttribute("y2", Convert(end.Y));
            options?.Apply(line);
            _current.AppendChild(line);

            _bounds.Peek().Expand(bounds);
            return bounds;
        }

        /// <inheritdoc />
        public Bounds Circle(Vector2 position, double radius, GraphicOptions options = null)
        {
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            position = CurrentTransform.Apply(position);
            var bounds = new Bounds(position - new Vector2(radius, radius), position + new Vector2(radius, radius));

            // Make the circle
            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", Convert(position.X));
            circle.SetAttribute("cy", Convert(position.Y));
            circle.SetAttribute("r", Convert(radius));
            options?.Apply(circle);
            _current.AppendChild(circle);

            _bounds.Peek().Expand(bounds);
            return bounds;
        }

        /// <inheritdoc />
        public Bounds Polyline(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var bounds = new ExpandableBounds();
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                bounds.Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append($"{Convert(tpt.X)},{Convert(tpt.Y)}");
            }

            // Creates the poly
            var poly = _document.CreateElement("polyline", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());

            _bounds.Peek().Expand(bounds);
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public Bounds Polygon(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var bounds = new ExpandableBounds();
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                bounds.Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append($"{Convert(tpt.X)},{Convert(tpt.Y)}");
            }

            // Create the element
            var poly = _document.CreateElement("polygon", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());

            _bounds.Peek().Expand(bounds);
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public Bounds SmoothBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var bounds = new ExpandableBounds();
            StringBuilder sb = new();
            int index = -1;
            foreach (IGrouping<int, Vector2> group in points.GroupBy(p => { index++; return index < 1 ? 0 : (index < 4) ? 1 : 1 + index / 4; }))
            {
                var v = group.ToArray();
                if (sb.Length > 0)
                    sb.Append(' ');
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    bounds.Expand(v[i]);
                }
                switch (v.Length)
                {
                    case 1: sb.Append($"M{Convert(v[0])}"); break;
                    case 3: sb.Append($"C{Convert(v[0])} {Convert(v[1])} {Convert(v[2])}"); break;
                    default: sb.Append($"S{Convert(v[0])} {Convert(v[1])} {Convert(v[2])} {Convert(v[3])}"); break;
                }
            }

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());

            _bounds.Peek().Expand(bounds);
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public Bounds ClosedBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var bounds = new ExpandableBounds();
            var sb = new StringBuilder();
            int index = 2;
            foreach (var group in points.GroupBy(p => (index++) / 3))
            {
                var v = group.ToArray();
                if (sb.Length > 0)
                    sb.Append(' ');
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    bounds.Expand(v[i]);
                }
                switch (v.Length)
                {
                    case 1: sb.Append($"M{Convert(v[0])}"); break;
                    default: sb.Append($"C{Convert(v[0])} {Convert(v[1])} {Convert(v[2])}"); break;
                }
            }
            sb.Append("Z");

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());

            _bounds.Peek().Expand(bounds);
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public Bounds OpenBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var bounds = new ExpandableBounds();
            var sb = new StringBuilder();
            int index = 2;
            foreach (var group in points.GroupBy(p => (index++) / 3))
            {
                var v = group.ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    bounds.Expand(v[i]);
                }
                if (sb.Length > 0)
                    sb.Append(' ');
                switch (v.Length)
                {
                    case 1: sb.Append($"M{Convert(v[0])}"); break;
                    default: sb.Append($"C{Convert(v[0])} {Convert(v[1])} {Convert(v[2])}"); break;
                }
            }

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());

            _bounds.Peek().Expand(bounds);
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public Bounds Arc(Vector2 center, double startAngle, double endAngle, double radius, GraphicOptions options = null, int intermediatePoints = 0)
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
            return OpenBezier(pts, options);
        }

        /// <inheritdoc />
        public Bounds Ellipse(Vector2 center, double rx, double ry, GraphicOptions options = null)
        {
            double kx = rx * 0.552284749831;
            double ky = ry * 0.552284749831;
            return ClosedBezier(new Vector2[]
            {
                new(-rx, 0),
                new(-rx, -ky), new(-kx, -ry), new(0, -ry),
                new(kx, -ry), new(rx, -ky), new(rx, 0),
                new(rx, ky), new(kx, ry), new(0, ry),
                new(-kx, ry), new(-rx, ky), new(-rx, 0)
            }.Select(v => v + center), options);
        }

        /// <inheritdoc />
        public Bounds Text(string value, Vector2 location, Vector2 expand, GraphicOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);
            var bounds = Formatter.Format(_current, value, location, expand, options, Diagnostics);

            _bounds.Peek().Expand(bounds);
            return bounds;
        }

        /// <inheritdoc />
        public Bounds Path(Action<PathBuilder> action, GraphicOptions options = null)
        {
            if (action == null)
                return default;
            var builder = new PathBuilder(CurrentTransform);
            action(builder);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", builder.ToString());

            _bounds.Peek().Expand(builder.Bounds);
            return builder.Bounds;
        }

        /// <summary>
        /// Begins a new group.
        /// </summary>
        /// <param name="options">The options.</param>
        public void BeginGroup(GraphicOptions options = null, bool atStart = false)
        {
            var elt = _document.CreateElement("g", Namespace);
            options?.Apply(elt);
            if (atStart)
                _current.PrependChild(elt);
            else
                _current.AppendChild(elt);
            _current = elt;

            // Track the bounds of the group
            _bounds.Push(new());
        }

        /// <summary>
        /// Ends the last opened group.
        /// </summary>
        public Bounds EndGroup()
        {
            var group = _current;
            var parent = _current.ParentNode;

            if (RemoveEmptyGroups && group.ChildNodes.Count == 0)
                parent.RemoveChild(group);

            // Go to the parent bounds
            if (_bounds.Count > 1)
            {
                var bounds = _bounds.Pop();
                var cBounds = _bounds.Peek();
                cBounds.Expand(bounds);

                // Draw a rectangle on top of the group to show
                if (RenderBounds && group.ParentNode != null)
                {
                    string id = group.Attributes["id"]?.Value;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var b = bounds.Bounds;
                        var center = new Vector2(0.5 * (b.Left + b.Right), 0.5 * (b.Top + b.Bottom));
                        double left = b.Left - ExpandBounds, right = b.Right + ExpandBounds;
                        double top = b.Top - ExpandBounds, bottom = b.Bottom + ExpandBounds;

                        var g = _document.CreateElement("g", Namespace);
                        _current = g;
                        g.SetAttribute("id", group.Attributes["id"]?.Value);
                        g.SetAttribute("class", "bounds");
                        group.ParentNode.InsertAfter(_current, group);
                        Polygon(new Vector2[]
                        {
                            new(left, top), new(right, top),
                            new(right, bottom), new(left, bottom)
                        });
                        
                        // Show the ID in the middle of the box
                        Text(id, center, new());
                    }
                }
                _current = parent ?? group;
                return bounds.Bounds;
            }
            _current = parent ?? group;
            return _bounds.Peek().Bounds;
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
            var bounds = _bounds.Peek().Bounds;

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
