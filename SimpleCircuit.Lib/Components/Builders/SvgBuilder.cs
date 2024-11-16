using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.Markers;
using SimpleCircuit.Parser.SimpleTexts;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Parser.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Components.Builders
{
    /// <summary>
    /// An <see cref="IGraphicsBuilder"/> for making SVG format data.
    /// </summary>
    public class SvgBuilder : IGraphicsBuilder
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
        private readonly ExpandableBounds _bounds = new();
        private readonly Stack<Transform> _tf = new();

        /// <inheritdoc />
        public ISet<string> RequiredCSS { get; } = new HashSet<string>();

        /// <summary>
        /// Adds extra CSS after the required CSS (ordered).
        /// </summary>
        public IList<string> ExtraCSS { get; } = [];

        /// <inheritdoc />
        public Transform CurrentTransform => _tf.Peek();

        /// <inheritdoc />
        public Bounds Bounds => _bounds.Bounds;

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
        public SvgBuilder(IDiagnosticHandler diagnostics = null, ITextMeasurer measurer = null)
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);

            // Make sure we can track the bounds of our vector image
            Diagnostics = diagnostics;
            Measurer = measurer ?? new SkiaTextMeasurer();
        }

        /// <inheritdoc />
        public IGraphicsBuilder BeginTransform(Transform tf)
        {
            _tf.Push(tf.Apply(CurrentTransform));
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder EndTransform()
        {
            _tf.Pop();
            if (_tf.Count == 0)
                _tf.Push(Transform.Identity);
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder DrawXml(XmlNode description, IXmlDrawingContext context)
        {
            // Apply some scale if necessary
            bool success = true;
            success &= description.Attributes.ParseOptionalScalar("scale", Diagnostics, 1.0, out double scale);
            success &= description.Attributes.ParseOptionalScalar("rotate", Diagnostics, 0.0, out double rotate);
            success &= description.Attributes.ParseOptionalVector("offset", Diagnostics, new(), out var offset);
            if (!success)
                return this;

            bool transform = !rotate.IsZero() || !offset.IsZero() || !scale.Equals(1.0);
            if (transform)
                BeginTransform(new Transform(offset, Matrix2.Rotate(rotate) * scale));
            DrawXmlActions(description, context);

            // If labels were found, let's try drawing the labels
            if (context.Anchors.Count > 0 && context.Labels != null && context.Labels.Count > 0)
                new CustomLabelAnchorPoints([.. context.Anchors]).Draw(this, context.Labels);

            if (transform)
                EndTransform();
            return this;
        }
        private void DrawXmlActions(XmlNode parent, IXmlDrawingContext context)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (!EvaluateVariants(node.Attributes?["variant"]?.Value, context))
                    continue;

                // Depending on the node type, let's draw something!
                switch (node.Name)
                {
                    case "#comment": break;
                    case "line": DrawXmlLine(node); break;
                    case "circle": DrawXmlCircle(node); break;
                    case "path": DrawXmlPath(node); break;
                    case "polygon": DrawXmlPolygon(node); break;
                    case "polyline": DrawXmlPolyline(node); break;
                    case "rect": DrawXmlRectangle(node); break;
                    case "text": DrawXmlText(node, context); break;
                    case "variant":
                    case "v":
                        // Just recursive thingy
                        DrawXmlActions(node, context);
                        break;
                    case "label": DrawXmlLabelAnchor(node, context); break;
                    case "group":
                    case "g":
                        // Parse options
                        GraphicOptions options = new();
                        ParseGraphicOptions(options, node);
                        BeginGroup(options);
                        DrawXmlActions(node, context);
                        EndGroup();
                        break;
                    default:
                        Diagnostics?.Post(ErrorCodes.CouldNotRecognizeDrawingCommand, node.Name);
                        break;
                }
            }
        }
        private void DrawXmlLine(XmlNode node)
        {
            if (node.Attributes == null)
                return;
            bool success = true;
            GraphicOptions options = new();
            HashSet<Marker> startMarkers = null, endMarkers = null;
            success &= node.Attributes.ParseOptionalScalar("x1", Diagnostics, 0.0, out double x1);
            success &= node.Attributes.ParseOptionalScalar("y1", Diagnostics, 0.0, out double y1);
            success &= node.Attributes.ParseOptionalScalar("x2", Diagnostics, 0.0, out double x2);
            success &= node.Attributes.ParseOptionalScalar("y2", Diagnostics, 0.0, out double y2);
            if (!success)
                return;

            // Draw the line
            ParseGraphicOptions(options, node, Diagnostics, ref startMarkers, ref endMarkers);
            Line(new(x1, y1), new(x2, y2), options);
            DrawMarkers(startMarkers, new(x1, y1), new(x2 - x1, y2 - y1));
            DrawMarkers(endMarkers, new(x2, y2), new(x2 - x1, y2 - y1));
        }
        private void DrawXmlPolygon(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr.AsMemory());
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the polygon
            Polygon(points, options);
        }
        private void DrawXmlPolyline(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr.AsMemory());
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;
            if (points == null || points.Count <= 1)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node, Diagnostics, ref startMarkers, ref endMarkers);

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
        private void DrawXmlCircle(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("cx", Diagnostics, 0.0, out double cx);
            success &= node.Attributes.ParseOptionalScalar("cy", Diagnostics, 0.0, out double cy);
            success &= node.Attributes.ParseOptionalScalar("r", Diagnostics, 0.0, out double r);
            if (!success)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the circle
            Circle(new(cx, cy), r, options);
        }
        private void DrawXmlPath(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string pathData = node.Attributes?["d"]?.Value;
            if (string.IsNullOrWhiteSpace(pathData))
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node, Diagnostics, ref startMarkers, ref endMarkers);

            if (!string.IsNullOrWhiteSpace(pathData))
            {
                SvgPathDataParser.MarkerLocation start = default, end = default;
                Path(b =>
                {
                    var lexer = new SvgPathDataLexer(pathData.AsMemory());
                    start = SvgPathDataParser.Parse(lexer, b, Diagnostics);
                    end = new SvgPathDataParser.MarkerLocation(b.End, b.EndNormal);
                }, options);
                DrawMarkers(startMarkers, start.Location, start.Normal);
                DrawMarkers(endMarkers, end.Location, end.Normal);
            }
        }
        private void DrawXmlRectangle(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", Diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", Diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("width", Diagnostics, 0.0, out double width);
            success &= node.Attributes.ParseOptionalScalar("height", Diagnostics, 0.0, out double height);
            success &= node.Attributes.ParseOptionalScalar("rx", Diagnostics, double.NaN, out double rx);
            success &= node.Attributes.ParseOptionalScalar("ry", Diagnostics, double.NaN, out double ry);
            if (!success)
                return;

            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            // Draw the rectangle
            this.Rectangle(x, y, width, height, rx, ry, options);
        }
        private void DrawXmlText(XmlNode node, IXmlDrawingContext context)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", Diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", Diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", Diagnostics, 0.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", Diagnostics, 0.0, out double ny);
            success &= node.Attributes.ParseOptionalScalar("size", Diagnostics, 4.0, out double size);
            success &= node.Attributes.ParseOptionalScalar("line-spacing", Diagnostics, 1.5, out double lineSpacing);
            if (!success)
                return;

            string value = node.Attributes?["value"]?.Value;
            GraphicOptions options = new();
            ParseGraphicOptions(options, node);

            if (value != null && context != null)
                value = context.TransformText(value);
            Text(value, new Vector2(x, y), new Vector2(nx, ny), size: size, lineSpacing: lineSpacing, options: options);
        }
        private void DrawXmlLabelAnchor(XmlNode node, IXmlDrawingContext context)
        {
            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", Diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", Diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", Diagnostics, 0.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", Diagnostics, 0.0, out double ny);
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
                startMarkers ??= [];
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(startMarkers, markerToken.Content.ToString(), diagnostics);
            }

            markers = node.Attributes?["marker-end"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                endMarkers ??= [];
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

        /// <inheritdoc />
        public IGraphicsBuilder Line(Vector2 start, Vector2 end, GraphicOptions options = null)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);

            // Create the line
            var line = _document.CreateElement("line", Namespace);
            line.SetAttribute("x1", start.X.ToSVG());
            line.SetAttribute("y1", start.Y.ToSVG());
            line.SetAttribute("x2", end.X.ToSVG());
            line.SetAttribute("y2", end.Y.ToSVG());
            options?.Apply(line);
            _current.AppendChild(line);

            _bounds.Expand(start, end);
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder Circle(Vector2 position, double radius, GraphicOptions options = null)
        {
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            position = CurrentTransform.Apply(position);

            // Make the circle
            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", position.X.ToSVG());
            circle.SetAttribute("cy", position.Y.ToSVG());
            circle.SetAttribute("r", radius.ToSVG());
            options?.Apply(circle);
            _current.AppendChild(circle);

            _bounds.Expand(
                position - new Vector2(radius, radius),
                position + new Vector2(radius, radius));
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder Polyline(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                _bounds.Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(tpt.ToSVG());
            }

            // Creates the poly
            var poly = _document.CreateElement("polyline", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder Polygon(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                _bounds.Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(tpt.ToSVG());
            }

            // Create the element
            var poly = _document.CreateElement("polygon", Namespace);
            options?.Apply(poly);
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder SmoothBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            StringBuilder sb = new();
            int index = -1;
            foreach (var group in points.GroupBy(p => { index++; return index < 1 ? 0 : index < 4 ? 1 : 1 + index / 4; }))
            {
                var v = group.ToArray();
                if (sb.Length > 0)
                    sb.Append(' ');
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    _bounds.Expand(v[i]);
                }
                switch (v.Length)
                {
                    case 1: sb.Append($"M{v[0].ToSVG()}"); break;
                    case 3: sb.Append($"C{v[0].ToSVG()} {v[1].ToSVG()} {v[2].ToSVG()}"); break;
                    default: sb.Append($"S{v[0].ToSVG()} {v[1].ToSVG()} {v[2].ToSVG()} {v[3].ToSVG()}"); break;
                }
            }

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder ClosedBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var sb = new StringBuilder();
            int index = 2;
            foreach (var group in points.GroupBy(p => index++ / 3))
            {
                var v = group.ToArray();
                if (sb.Length > 0)
                    sb.Append(' ');
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    _bounds.Expand(v[i]);
                }
                switch (v.Length)
                {
                    case 1: sb.Append($"M{v[0].ToSVG()}"); break;
                    default: sb.Append($"C{v[0].ToSVG()} {v[1].ToSVG()} {v[2].ToSVG()}"); break;
                }
            }
            sb.Append("Z");

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder OpenBezier(IEnumerable<Vector2> points, GraphicOptions options = null)
        {
            var sb = new StringBuilder();
            int index = 2;
            foreach (var group in points.GroupBy(p => index++ / 3))
            {
                var v = group.ToArray();
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = CurrentTransform.Apply(v[i]);
                    _bounds.Expand(v[i]);
                }
                if (sb.Length > 0)
                    sb.Append(' ');
                switch (v.Length)
                {
                    case 1: sb.Append($"M{v[0].ToSVG()}"); break;
                    default: sb.Append($"C{v[0].ToSVG()} {v[1].ToSVG()} {v[2].ToSVG()}"); break;
                }
            }

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, GraphicOptions options = null)
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

        public IGraphicsBuilder Text(ISpan span, Vector2 location, Vector2 expand, GraphicOptions options = null)
        {
            if (span is null)
                return this;

            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            // Create the text element
            var text = _document.CreateElement("text", Namespace);
            options?.Apply(text);
            _current.AppendChild(text);

            // Compute the location based on the location and expansion
            var bounds = span.Bounds.Bounds;
            double y = location.Y, x = location.X;
            if (expand.Y.IsZero())
                y = y - bounds.Height * 0.5 - bounds.Top;
            else if (expand.Y < 0)
                y -= bounds.Bottom;
            else
                y -= bounds.Top;
            if (expand.X.IsZero())
                x = x - bounds.Width * 0.5 - bounds.Left;
            else if (expand.X < 0)
                x -= bounds.Right;
            else
                x -= bounds.Left;

            // Make the SVG for the text
            BuildTextSVG(new(x, y), span, _current, text);

            // Return the offset bounds
            _bounds.Expand(bounds + new Vector2(x, y));
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder Text(string value, Vector2 location, Vector2 expand, double size = 4.0, double lineSpacing = 1.5, GraphicOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            // Create the text element
            var text = _document.CreateElement("text", Namespace);
            options?.Apply(text);
            _current.AppendChild(text);

            // Parse the text value
            value = value.Replace("<", "&lt;").Replace(">", "&gt;");
            var lexer = new SimpleTextLexer(value);
            var context = new SimpleTextContext(_current, Measurer)
            {
                FontSize = size,
                LineSpacing = lineSpacing,
                Text = text,
                Align = expand.X
            };
            var span = SimpleTextParser.Parse(lexer, context);
            var bounds = span.Bounds.Bounds;

            // Compute the location based on the location and expansion
            double y = location.Y, x = location.X;
            if (expand.Y.IsZero())
                y = y - bounds.Height * 0.5 - bounds.Top;
            else if (expand.Y < 0)
                y -= bounds.Bottom;
            else
                y -= bounds.Top;
            if (expand.X.IsZero())
                x = x - bounds.Width * 0.5 - bounds.Left;
            else if (expand.X < 0)
                x -= bounds.Right;
            else
                x -= bounds.Left;

            // Make the SVG for the text
            BuildTextSVG(new(x, y), span, _current, text);

            // Return the offset bounds
            var r = new Vector2(x, y) + bounds;
            _bounds.Expand(r);
            return this;
        }

        private void BuildTextSVG(Vector2 offset, ISpan span, XmlNode parent, XmlElement current)
        {
            if (span is null)
                return;
            switch (span)
            {
                case TextSpan textSpan:
                    {
                        // Make a span at the specified location
                        var element = _document.CreateElement("tspan", Namespace);
                        element.SetAttribute("style", $"font-family:{textSpan.FontFamily};font-size:{textSpan.Size.ToSVG()}pt;font-weight:{(textSpan.Bold ? "bold" : "normal")};");
                        element.SetAttribute("x", offset.X + textSpan.Offset.X.ToSVG());
                        element.SetAttribute("y", offset.Y + textSpan.Offset.Y.ToSVG());
                        element.InnerXml = textSpan.Content;
                        current.AppendChild(element);
                    }
                    break;

                case LineSpan lineSpan:
                    {
                        foreach (var s in lineSpan)
                            BuildTextSVG(offset, s, parent, current);
                    }
                    break;

                case MultilineSpan multilineSpan:
                    {
                        foreach (var s in multilineSpan)
                            BuildTextSVG(offset, s, parent, current);
                    }
                    break;

                case SubscriptSuperscriptSpan subSuperSpan:
                    {
                        BuildTextSVG(offset, subSuperSpan.Base, parent, current);
                        BuildTextSVG(offset, subSuperSpan.Sub, parent, current);
                        BuildTextSVG(offset, subSuperSpan.Super, parent, current);
                    }
                    break;

                case OverlineSpan overlineSpan:
                    {
                        Path(b => b
                            .MoveTo(offset + overlineSpan.Start)
                            .LineTo(offset + overlineSpan.End),
                            new() { Style = $"stroke-width:{overlineSpan.Thickness.ToSVG()}pt;fill:none;" });
                    }
                    break;

                case UnderlineSpan underlineSpan:
                    {
                        Path(b => b
                            .MoveTo(offset + underlineSpan.Start)
                            .LineTo(offset + underlineSpan.End),
                            new() { Style = $"stroke-width:{underlineSpan.Thickness.ToSVG()}pt;fill:none;" });
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public IGraphicsBuilder Path(Action<IPathBuilder> action, GraphicOptions options = null)
        {
            if (action == null)
                return this;
            var builder = new SvgPathBuilder(CurrentTransform, _bounds);
            action(builder);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            options?.Apply(path);
            _current.AppendChild(path);
            path.SetAttribute("d", builder.ToString());
            return this;
        }

        /// <summary>
        /// Begins a new group.
        /// </summary>
        /// <param name="options">The options.</param>
        public IGraphicsBuilder BeginGroup(GraphicOptions options = null, bool atStart = false)
        {
            var elt = _document.CreateElement("g", Namespace);
            options?.Apply(elt);
            if (atStart)
                _current.PrependChild(elt);
            else
                _current.AppendChild(elt);
            _current = elt;
            return this;
        }

        /// <summary>
        /// Ends the last opened group.
        /// </summary>
        public IGraphicsBuilder EndGroup()
        {
            var group = _current;
            var parent = _current.ParentNode;

            if (RemoveEmptyGroups && group.ChildNodes.Count == 0)
                parent.RemoveChild(group);
            _current = parent ?? group;
            return this;
        }

        /// <summary>
        /// Gets the SVG xml-document.
        /// </summary>
        /// <returns>The document.</returns>
        public XmlDocument GetDocument()
        {
            var svg = _document.DocumentElement;

            // Add stylesheet info if necessary
            var styleElt = _document.CreateElement("style", Namespace);
            List<string> style =
            [
                Properties.Resources.DefaultStyle,
                .. RequiredCSS,
                .. ExtraCSS
            ];
            styleElt.InnerText = string.Join(Environment.NewLine, style);
            svg.PrependChild(styleElt);

            // Try to get the bounds of this
            var bounds = _bounds.Bounds.Expand(Margin);

            // Apply a margin along the edges
            svg.SetAttribute("width", ((int)bounds.Width * 5).ToString());
            svg.SetAttribute("height", ((int)bounds.Height * 5).ToString());
            svg.SetAttribute("viewBox", $"{bounds.Left.ToSVG()} {bounds.Top.ToSVG()} {bounds.Width.ToSVG()} {bounds.Height.ToSVG()}");

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
            if (content.IndexOfAny(['\r', '\n']) >= 0)
                content = Environment.NewLine + content + Environment.NewLine;
            if (content.IndexOfAny(['"', '\'', '<', '>', '&']) >= 0)
                elt.AppendChild(_document.CreateCDataSection(content));
            else
                elt.InnerText = content;
            metadata.AppendChild(elt);
        }
    }
}
