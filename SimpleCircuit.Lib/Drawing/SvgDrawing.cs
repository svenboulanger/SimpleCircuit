using SimpleCircuit.Components;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Markers;
using SimpleCircuit.Parser.Markers;
using SimpleCircuit.Parser.SimpleTexts;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Parser.Variants;
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

        /// <summary>
        /// The default font size.
        /// </summary>
        public const double DefaultFontSize = 4.0;

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
        /// Gets the text measurer.
        /// </summary>
        public ITextMeasurer Measurer { get; }

        /// <summary>
        /// Creates a new SVG drawing instance.
        /// </summary>
        public SvgDrawing(IDiagnosticHandler diagnostics = null, ITextMeasurer measurer = null)
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);

            // Make sure we can track the bounds of our vector image
            _bounds = new();
            _bounds.Push(new());
            Diagnostics = diagnostics;
            Measurer = measurer ?? new SkiaTextMeasurer();
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
            bool success = true;
            success &= description.Attributes.ParseOptionalScalar("scale", diagnostics, 1.0, out double scale);
            success &= description.Attributes.ParseOptionalScalar("rotate", diagnostics, 0.0, out double rotate);
            success &= description.Attributes.ParseOptionalVector("offset", diagnostics, new(), out var offset);
            if (!success)
                return;

            bool transform = !rotate.IsZero() || !offset.IsZero() || !scale.Equals(1.0);
            if (transform)
                BeginTransform(new Transform(offset, Matrix2.Rotate(rotate) * scale));
            DrawXmlActions(description, context, diagnostics);

            // If labels were found, let's try drawing the labels
            if (context.Anchors.Count > 0 && context.Labels != null && context.Labels.Count > 0)
                new CustomLabelAnchorPoints(context.Anchors.ToArray()).Draw(this, context.Labels);

            if (transform)
                EndTransform();
        }
        private void DrawXmlActions(XmlNode parent, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (!EvaluateVariants(node.Attributes?["variant"]?.Value, context))
                    continue;

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
                    case "variant":
                    case "v":
                        // Just recursive thingy
                        DrawXmlActions(node, context, diagnostics);
                        break;
                    case "label": DrawXmlLabelAnchor(node, context, diagnostics); break;
                    case "group":
                    case "g":
                        // Parse options
                        GraphicOptions options = new();
                        ParseGraphicOptions(options, node);
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
            bool success = true;
            GraphicOptions options = new();
            HashSet<Marker> startMarkers = null, endMarkers = null;
            success &= node.Attributes.ParseOptionalScalar("x1", diagnostics, 0.0, out double x1);
            success &= node.Attributes.ParseOptionalScalar("y1", diagnostics, 0.0, out double y1);
            success &= node.Attributes.ParseOptionalScalar("x2", diagnostics, 0.0, out double x2);
            success &= node.Attributes.ParseOptionalScalar("y2", diagnostics, 0.0, out double y2);
            if (!success)
                return;

            // Draw the line
            ParseGraphicOptions(options, node, diagnostics, ref startMarkers, ref endMarkers);
            Line(new(x1, y1), new(x2, y2), options);
            DrawMarkers(startMarkers, new(x1, y1), new(x2 - x1, y2 - y1));
            DrawMarkers(endMarkers, new(x2, y2), new(x2 - x1, y2 - y1));
        }
        private void DrawXmlPolygon(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr.AsMemory());
            List<Vector2> points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
            if (points == null || points.Count <= 1)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the polygon
            Polygon(points, options);
        }
        private void DrawXmlPolyline(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr.AsMemory());
            List<Vector2> points = SvgPathDataParser.ParsePoints(lexer, diagnostics);
            if (points == null || points.Count <= 1)
                return;
            if (points == null || points.Count <= 1)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node, diagnostics, ref startMarkers, ref endMarkers);

            // Draw the polyline
            Polyline(points, options);
            if (points.Count > 1)
            {
                DrawMarkers(startMarkers, points[0], points[1] - points[0]);
                DrawMarkers(endMarkers, points[0], points[0] - points[^1]);
            }
            else
            {
                DrawMarkers(startMarkers, points[0], new(1, 0));
                DrawMarkers(endMarkers, points[0], new(1, 0));
            }
        }
        private void DrawXmlCircle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("cx", diagnostics, 0.0, out double cx);
            success &= node.Attributes.ParseOptionalScalar("cy", diagnostics, 0.0, out double cy);
            success &= node.Attributes.ParseOptionalScalar("r", diagnostics, 0.0, out double r);
            if (!success)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the circle
            Circle(new(cx, cy), r, options);
        }
        private void DrawXmlPath(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string pathData = node.Attributes?["d"]?.Value;
            if (string.IsNullOrWhiteSpace(pathData))
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node, diagnostics, ref startMarkers, ref endMarkers);

            if (!string.IsNullOrWhiteSpace(pathData))
            {
                SvgPathDataParser.MarkerLocation start = default, end = default;
                Path(b =>
                {
                    var lexer = new SvgPathDataLexer(pathData.AsMemory());
                    start = SvgPathDataParser.Parse(lexer, b, diagnostics);
                    end = new SvgPathDataParser.MarkerLocation(b.End, b.EndNormal);
                }, options);
                DrawMarkers(startMarkers, start.Location, start.Normal);
                DrawMarkers(endMarkers, end.Location, end.Normal);
            }
        }
        private void DrawXmlRectangle(XmlNode node, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("width", diagnostics, 0.0, out double width);
            success &= node.Attributes.ParseOptionalScalar("height", diagnostics, 0.0, out double height);
            success &= node.Attributes.ParseOptionalScalar("rx", diagnostics, double.NaN, out double rx);
            success &= node.Attributes.ParseOptionalScalar("ry", diagnostics, double.NaN, out double ry);
            if (!success)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the rectangle
            this.Rectangle(x, y, width, height, rx, ry, options);
        }
        private void DrawXmlText(XmlNode node, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", diagnostics, 0.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", diagnostics, 0.0, out double ny);
            success &= node.Attributes.ParseOptionalScalar("size", diagnostics, 4.0, out double size);
            if (!success)
                return;

            string value = node.Attributes?["value"]?.Value;
            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            if (value != null && context != null)
                value = context.TransformText(value);
            Text(value, new Vector2(x, y), new Vector2(nx, ny), size, options);
        }
        private void DrawXmlLabelAnchor(XmlNode node, IXmlDrawingContext context, IDiagnosticHandler diagnostics)
        {
            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", diagnostics, 0.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", diagnostics, 0.0, out double ny);
            if (!success)
                return;

            var options = new GraphicOptions();
            ParseGraphicOptions(options, node);
            context.Anchors.Add(new LabelAnchorPoint(new(x, y), new(nx, ny), options));
        }
        private void ParseGraphicOptions(GraphicOptions options, XmlNode node)
        {
            options.Style = node.Attributes?["style"]?.Value;
            string classes = node.Attributes?["class"]?.Value;
            if (!string.IsNullOrWhiteSpace(classes))
            {
                foreach (string name in classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    options.Classes.Add(name);
            }
        }
        private void ParseGraphicOptions(GraphicOptions options, XmlNode node, IDiagnosticHandler diagnostics, ref HashSet<Marker> startMarkers, ref HashSet<Marker> endMarkers)
        {
            ParseGraphicOptions(options, node);

            string markers = node.Attributes?["marker-start"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                startMarkers ??= new HashSet<Marker>();
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(startMarkers, markerToken.Content.ToString(), diagnostics);
            }

            markers = node.Attributes?["marker-end"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                endMarkers ??= new HashSet<Marker>();
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(endMarkers, markerToken.Content.ToString(), diagnostics);
            }
        }
        private void AddMarker(HashSet<Marker> markers, string value, IDiagnosticHandler diagnostics)
        {
            switch (value)
            {
                case "arrow": markers.Add(new Arrow()); break;
                case "rarrow": markers.Add(new ReverseArrow()); break;
                case "erd-many": markers.Add(new ERDMany()); break;
                case "erd-one": markers.Add(new ERDOne()); break;
                case "erd-one-many": markers.Add(new ERDOneMany()); break;
                case "erd-only-one": markers.Add(new ERDOnlyOne()); break;
                case "erd-zero-many": markers.Add(new ERDZeroMany()); break;
                case "erd-zero-one": markers.Add(new ERDZeroOne()); break;
                case "plus": markers.Add(new Plus()); break;
                case "minus": markers.Add(new Minus()); break;
                case "slash": markers.Add(new Slash()); break;
                default:
                    diagnostics?.Post(ErrorCodes.InvalidMarker, value);
                    break;
            }
        }
        private void DrawMarkers(HashSet<Marker> markers, Vector2 location, Vector2 orientation)
        {
            if (markers == null)
                return;
            if (orientation.IsZero())
                orientation = new(1, 0);
            else
                orientation /= orientation.Length;
            foreach (var marker in markers)
            {
                marker.Location = location;
                marker.Orientation = orientation;
                marker.Draw(this);
            }
        }
        private bool EvaluateVariants(string value, IVariantContext context)
        {
            if (value is null)
                return true;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var lexer = new VariantLexer(value);
            return VariantParser.Parse(lexer, context);
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
        public Bounds Text(string value, Vector2 location, Vector2 expand, double size = 4.0, GraphicOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            // Parse the text and layout the elements
            var text = _document.CreateElement("text", Namespace);
            options?.Apply(text);
            _current.AppendChild(text);
            var lexer = new SimpleTextLexer(value);
            var context = new SimpleTextContext(text, Measurer)
            {
                FontSize = size
            };
            SimpleTextParser.Parse(lexer, context);
            var bounds = context.Finish(location, expand);

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
                    string id = group.Attributes?["id"]?.Value;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var b = bounds.Bounds;
                        var center = new Vector2(0.5 * (b.Left + b.Right), 0.5 * (b.Top + b.Bottom));
                        double left = b.Left - ExpandBounds, right = b.Right + ExpandBounds;
                        double top = b.Top - ExpandBounds, bottom = b.Bottom + ExpandBounds;

                        var g = _document.CreateElement("g", Namespace);
                        _current = g;
                        g.SetAttribute("class", "bounds");
                        group.ParentNode.InsertAfter(_current, group);
                        Polygon(new Vector2[]
                        {
                            new(left, top), new(right, top),
                            new(right, bottom), new(left, bottom)
                        });
                        
                        // Show the ID in the middle of the box
                        Text(id.Replace("_", "\\_"), center, new());
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
        public XmlDocument GetDocument(string style = null)
        {
            var svg = _document.DocumentElement;

            // Add stylesheet info if necessary
            if (!string.IsNullOrWhiteSpace(style))
            {
                var styleElt = _document.CreateElement("style", Namespace);
                styleElt.InnerText = style;
                svg.PrependChild(styleElt);
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
            string result = Math.Round(value, 2).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            int length = result.Length - 1;
            while (result[length] == '0')
                length--;
            if (result[length] == '.')
                return result[..length];
            return result[..(length + 1)];
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
