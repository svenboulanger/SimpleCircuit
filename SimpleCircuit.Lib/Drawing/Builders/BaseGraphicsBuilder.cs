using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Markers;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Parser.Variants;
using System;
using System.Collections.Generic;
using System.Xml;
using SimpleCircuit.Parser.Styles;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Components;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Components.Markers;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// A base <see cref="IGraphicsBuilder"/>.
    /// </summary>
    public abstract class BaseGraphicsBuilder : IGraphicsBuilder
    {
        private readonly Stack<Transform> _tf = new();
        private readonly ExpandableBounds _globalBounds = new();
        private readonly Stack<ExpandableBounds> _bounds = new();

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public Transform CurrentTransform => _tf.Peek();

        /// <inheritdoc />
        public Bounds Bounds => _globalBounds.Bounds;

        /// <inheritdoc />
        public ITextFormatter TextFormatter { get; }

        /// <inheritdoc />
        public IStyle Style { get; private set; }

        /// <summary>
        /// Creates a new <see cref="BaseGraphicsBuilder"/>.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        protected BaseGraphicsBuilder(ITextFormatter formatter, IStyle style, IDiagnosticHandler diagnostics)
        {
            TextFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            Style = style ?? throw new ArgumentNullException(nameof(style));
            Diagnostics = diagnostics;
            _tf.Push(Transform.Identity);
            _bounds.Push(new());
        }

        /// <summary>
        /// Expands all currently active bounds by the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        protected void Expand(Vector2 point)
        {
            _globalBounds.Expand(point);
            foreach (var b in _bounds)
                b.Expand(point);
        }

        /// <summary>
        /// Expands all currently active bounds by the given points.
        /// </summary>
        /// <param name="points">The points.</param>
        protected void Expand(params Vector2[] points)
        {
            if (points is null)
                return;
            for (int i = 0; i < points.Length; i++)
                Expand(points[i]);
        }

        /// <summary>
        /// Expands all currently active bounds by the given bound.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        protected void Expand(Bounds bounds)
        {
            Expand(new Vector2(bounds.Left, bounds.Top));
            Expand(new Vector2(bounds.Right, bounds.Bottom));
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
        public IGraphicsBuilder BeginBounds()
        {
            _bounds.Push(new());
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder EndBounds(out Bounds bounds)
        {
            bounds = _bounds.Pop().Bounds;
            return this;
        }

        /// <inheritdoc />
        public IGraphicsBuilder ExpandBounds(Vector2 point)
        {
            point = CurrentTransform.Apply(point);
            Expand(point);
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
                new CustomLabelAnchorPoints([.. context.Anchors]).Draw(this, context, Style);

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
                        var style = ParseStyleModifier(node);
                        var oldStyle = Style;
                        Style = style?.Apply(Style) ?? Style;
                        BeginGroup();
                        DrawXmlActions(node, context);
                        EndGroup();
                        Style = oldStyle;
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
            HashSet<Marker> startMarkers = null, endMarkers = null;
            success &= node.Attributes.ParseOptionalScalar("x1", Diagnostics, 0.0, out double x1);
            success &= node.Attributes.ParseOptionalScalar("y1", Diagnostics, 0.0, out double y1);
            success &= node.Attributes.ParseOptionalScalar("x2", Diagnostics, 0.0, out double x2);
            success &= node.Attributes.ParseOptionalScalar("y2", Diagnostics, 0.0, out double y2);
            if (!success)
                return;

            // Draw the line
            var modifier = ParseStyleModifier(node, ref startMarkers, ref endMarkers);
            var style = modifier?.Apply(Style) ?? Style;
            Line(new(x1, y1), new(x2, y2), style);
            DrawMarkers(startMarkers, new(x1, y1), new(x2 - x1, y2 - y1), style);
            DrawMarkers(endMarkers, new(x2, y2), new(x2 - x1, y2 - y1), style);
        }

        private void DrawXmlPolygon(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr);
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;

            var modifier = ParseStyleModifier(node);
            var style = modifier?.Apply(Style) ?? Style;

            // Draw the polygon
            Polygon(points, style);
        }

        private void DrawXmlPolyline(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr);
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;
            if (points == null || points.Count <= 1)
                return;

            var modifier = ParseStyleModifier(node, ref startMarkers, ref endMarkers);
            var style = modifier?.Apply(Style) ?? Style;

            // Draw the polyline
            Polyline(points, style);
            if (points.Count > 1)
            {
                DrawMarkers(startMarkers, points[0], points[1] - points[0], style);
                DrawMarkers(endMarkers, points[0], points[0] - points[^1], style);
            }
            else
            {
                DrawMarkers(startMarkers, points[0], new(1, 0), style);
                DrawMarkers(endMarkers, points[0], new(1, 0), style);
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

            var modifier = ParseStyleModifier(node);
            var style = modifier?.Apply(Style) ?? Style;

            // Draw the circle
            Circle(new(cx, cy), r, style);
        }

        private void DrawXmlPath(XmlNode node)
        {
            if (node.Attributes == null)
                return;

            HashSet<Marker> startMarkers = null, endMarkers = null;
            string pathData = node.Attributes?["d"]?.Value;
            if (string.IsNullOrWhiteSpace(pathData))
                return;

            var modifier = ParseStyleModifier(node, ref startMarkers, ref endMarkers);
            var style = modifier?.Apply(Style) ?? Style;

            if (!string.IsNullOrWhiteSpace(pathData))
            {
                SvgPathDataParser.MarkerLocation start = default, end = default;
                Path(b =>
                {
                    var lexer = new SvgPathDataLexer(pathData);
                    start = SvgPathDataParser.Parse(lexer, b, Diagnostics);
                    end = new SvgPathDataParser.MarkerLocation(b.End, b.EndNormal);
                }, style);
                DrawMarkers(startMarkers, start.Location, start.Normal, style);
                DrawMarkers(endMarkers, end.Location, end.Normal, style);
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

            var modifier = ParseStyleModifier(node);
            var style = modifier?.Apply(Style) ?? Style;

            // Draw the rectangle
            this.Rectangle(x, y, width, height, style, rx, ry);
        }

        private void DrawXmlText(XmlNode node, IXmlDrawingContext context)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", Diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", Diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", Diagnostics, 1.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", Diagnostics, 0.0, out double ny);
            if (!success)
                return;

            // Get the style
            var modifier = ParseStyleModifier(node, asText: true);
            var style = modifier?.Apply(Style) ?? Style;

            // Get the text value
            string value = node.Attributes?["value"]?.Value;
            if (value != null && context != null)
                value = context.TransformText(value);
            var span = TextFormatter.Format(value, style);

            // Now let's transform the text location and orientation if necessary
            var type = GetTextOrientationType(node);

            // Also modify the position depending on an additional flag
            var location = new Vector2(x, y);
            var anchorOrientation = GetExpansion(node);

            // Also introduce an anchor mode, that allows us to transform the expandOffset
            string mode = node.Attributes?["transform-anchor"]?.Value;
            if (mode is null)
            {
                if ((type & TextOrientationType.Transformed) == 0)
                    anchorOrientation = CurrentTransform.ApplyDirection(anchorOrientation);
            }
            else
            {
                switch (mode?.ToLower())
                {
                    case "true":
                        anchorOrientation = CurrentTransform.ApplyDirection(anchorOrientation);
                        break;

                    case "false":
                        break;

                    default:
                        if ((type & TextOrientationType.Transformed) == 0)
                            anchorOrientation = CurrentTransform.ApplyDirection(anchorOrientation);
                        break;
                }
            }

            // Now let's calculate the real offset
            {
                double offsetX = 0, offsetY = 0;
                if (!anchorOrientation.X.IsNaN())
                {
                    if (anchorOrientation.X.IsZero())
                        offsetX -= 0.5 * (span.Bounds.Bounds.Left + span.Bounds.Bounds.Right);
                    else if (anchorOrientation.X > 0)
                        offsetX -= span.Bounds.Bounds.Left;
                    else
                        offsetX -= span.Bounds.Bounds.Right;
                }
                if (!anchorOrientation.Y.IsNaN())
                {
                    if (anchorOrientation.Y.IsZero())
                        offsetY -= 0.5 * (span.Bounds.Bounds.Top + span.Bounds.Bounds.Bottom);
                    else if (anchorOrientation.Y > 0)
                        offsetY -= span.Bounds.Bounds.Top;
                    else
                        offsetY -= span.Bounds.Bounds.Bottom;
                }
                if ((type & TextOrientationType.Transformed) == 0)
                    location += CurrentTransform.Matrix.Inverse * new Vector2(offsetX, offsetY);
                else
                    location += new Vector2(offsetX, offsetY);
            }

            Text(span, location, new Vector2(nx, ny), type);
        }

        private void DrawXmlLabelAnchor(XmlNode node, IXmlDrawingContext context)
        {
            bool success = true;
            success &= node.Attributes.ParseOptionalScalar("x", Diagnostics, 0.0, out double x);
            success &= node.Attributes.ParseOptionalScalar("y", Diagnostics, 0.0, out double y);
            success &= node.Attributes.ParseOptionalScalar("nx", Diagnostics, 1.0, out double nx);
            success &= node.Attributes.ParseOptionalScalar("ny", Diagnostics, 0.0, out double ny);

            if (!success)
                return;
            context.Anchors.Add(new LabelAnchorPoint(new(x, y), GetExpansion(node), new(nx, ny), GetTextOrientationType(node)));
        }
        private static TextOrientationType GetTextOrientationType(XmlNode node)
        {
            // Now let's transform the text location and orientation if necessary
            string mode = node.Attributes?["mode"]?.Value;
            mode ??= "upright-transformed";
            return mode.ToLower() switch
            {
                "transformed" => TextOrientationType.Transformed,
                "upright" => TextOrientationType.Upright,
                "upright-transformed" => TextOrientationType.UprightTransformed,
                _ => TextOrientationType.Transformed
            };
        }
        private static Vector2 GetExpansion(XmlNode node)
        {
            // Also modify the position depending on an additional flag
            var anchorOrientation = Vector2.NaN;
            string mode = node.Attributes?["anchor"]?.Value;
            if (mode is not null)
            {
                switch (mode?.ToLower())
                {
                    case "center": anchorOrientation = new(0, 0); break;
                    case "center-baseline": anchorOrientation = new(0, double.NaN); break; // Horizontally centered anchor
                    case "baseline-center": anchorOrientation = new(double.NaN, 0); break; // Vertically centered anchor
                    case "left": anchorOrientation = new(1, 0); break;
                    case "top": anchorOrientation = new(0, 1); break;
                    case "right": anchorOrientation = new(-1, 0); break;
                    case "bottom": anchorOrientation = new(0, -1); break;
                    case "bottom-left":
                    case "bottomleft": anchorOrientation = new(1, -1); break;
                    case "bottom-right":
                    case "bottomright": anchorOrientation = new(-1, -1); break;
                    case "top-left":
                    case "topleft": anchorOrientation = new(1, 1); break;
                    case "top-right":
                    case "topright": anchorOrientation = new(-1, 1); break;
                    case "baseline":
                    case "baseline-baseline":
                    case "origin": anchorOrientation = Vector2.NaN; break;
                }
            }
            return anchorOrientation;
        }

        private IStyleModifier ParseStyleModifier(XmlNode node, bool asText = false)
        {
            IStyleModifier result = null;
            if (node.Attributes is not null)
            {
                // Color from attributes
                // Foreground color
                string color = node.Attributes["color"]?.Value;
                color ??= node.Attributes["fg"]?.Value;
                color ??= node.Attributes["foreground"]?.Value;

                // Background color
                string bgColor = node.Attributes["background"]?.Value;
                bgColor = node.Attributes["bg"]?.Value;

                // Some dependency on whether we want to parse as text
                if (asText)
                    color ??= node.Attributes["fill"]?.Value;
                else
                {
                    color ??= node.Attributes["stroke"]?.Value;
                    bgColor ??= node.Attributes["fill"]?.Value;
                }

                // Store
                if (color is not null || bgColor is not null)
                    result = result.Append(new ColorStyleModifier(color, bgColor));

                // StrokeDashArray from attributes
                string strokeDashArray = node.Attributes["stroke-dasharray"]?.Value;
                if (strokeDashArray is not null)
                    result = result.Append(new StrokeDashArrayStyleModifier(strokeDashArray));

                // Stroke width from attributes
                string strokeWidth = node.Attributes["stroke-width"]?.Value;
                strokeWidth ??= node.Attributes["thickness"]?.Value;
                if (strokeWidth is not null)
                {
                    if (double.TryParse(strokeWidth, out double thickness))
                    {
                        if (strokeWidth.EndsWith("px"))
                            thickness *= 4.0 / 3.0;
                        result = result.Append(new StrokeWidthStyleModifier(thickness));
                    }
                }

                // Font-family from attributes
                string fontFamily = node.Attributes["font-family"]?.Value;
                if (fontFamily is not null)
                    result = result.Append(new FontFamilyStyleModifier(fontFamily));

                // Font-size from attributes
                string fontSize = node.Attributes["font-size"]?.Value;
                if (fontSize is not null)
                {
                    if (double.TryParse(fontSize, out double size))
                    {
                        if (fontSize.EndsWith("px"))
                            size *= 4.0 / 3.0;
                        result = result.Append(new FontSizeStyleModifier(size / 3.0 * 4.0));
                    }
                }

                // Opacity from attributes
                string opacity = node.Attributes["opacity"]?.Value;
                if (opacity is not null)
                {
                    if (double.TryParse(opacity, out double scale))
                        result = result.Append(new OpacityStyleModifier(scale, scale));
                }

                opacity = node.Attributes["foreground-opacity"]?.Value;
                opacity ??= node.Attributes["fgo"]?.Value;
                if (opacity is not null)
                {
                    if (double.TryParse(opacity, out double scale))
                        result = result.Append(new OpacityStyleModifier(scale, null));
                }

                opacity = node.Attributes["background-opacity"]?.Value;
                opacity ??= node.Attributes["bgo"]?.Value;
                if (opacity is not null)
                {
                    if (double.TryParse(opacity, out double scale))
                        result = result.Append(new OpacityStyleModifier(null, scale));
                }

                // Justification from attributes
                string justification = node.Attributes["justification"]?.Value;
                justification ??= node.Attributes["justify"]?.Value;
                justification ??= node.Attributes["text-align"]?.Value;
                if (justification is not null)
                {
                    switch (justification.ToLower())
                    {
                        case "left":
                        case "text-left":
                            result = result.Append(new JustificationStyleModifier(1.0));
                            break;

                        case "center":
                        case "text-center":
                            result = result.Append(new JustificationStyleModifier(0.0));
                            break;

                        case "right":
                        case "text-right":
                            result = result.Append(new JustificationStyleModifier(-1.0));
                            break;

                        default:
                            if (double.TryParse(justification, out double scale))
                                result = result.Append(new JustificationStyleModifier(scale));
                            break;
                    }
                }
            }

            // Parse the style string    
            var style = node.Attributes["style"]?.Value;
            if (!string.IsNullOrWhiteSpace(style))
            {
                var lexer = new StylesLexer(style);
                while (lexer.Type != Parser.Styles.TokenType.EndOfContent)
                {
                    // First we expect a key
                    if (!lexer.Branch(Parser.Styles.TokenType.Key, out var keyToken))
                    {
                        lexer.Skip(~Parser.Styles.TokenType.Semicolon);
                        lexer.Next();
                        continue;
                    }

                    // Then we expect a colon
                    if (!lexer.Branch(Parser.Styles.TokenType.Colon))
                    {
                        lexer.Skip(~Parser.Styles.TokenType.Semicolon);
                        lexer.Next();
                        continue;
                    }

                    // The value can be anything
                    var start = lexer.Track();
                    int nestedLevel = 0;
                    while (lexer.Check(~Parser.Styles.TokenType.Semicolon) && nestedLevel == 0)
                    {
                        switch (lexer.Type)
                        {
                            case Parser.Styles.TokenType.Parenthesis:
                                if (lexer.Token.Content.Span[0] == '(')
                                    nestedLevel++;
                                else
                                    nestedLevel--;
                                lexer.Next();
                                break;

                            default:
                                lexer.Next();
                                break;
                        }
                    }

                    // Get the full value
                    var value = lexer.GetTracked(start);
                    switch (keyToken.Content.ToString().ToLower())
                    {
                        case "color":
                        case "foreground":
                        case "fg":
                            result = result.Append(new ColorStyleModifier(value.Content.ToString(), null));
                            break;

                        case "stroke":
                            if (!asText)
                                result = result.Append(new ColorStyleModifier(value.Content.ToString(), null));
                            break;

                        case "fill":
                            if (asText)
                                result = result.Append(new ColorStyleModifier(value.Content.ToString(), null));
                            else
                                result = result.Append(new ColorStyleModifier(null, value.Content.ToString()));
                            break;

                        case "background":
                        case "bg":
                            result = result.Append(new ColorStyleModifier(null, value.Content.ToString()));
                            break;

                        case "stroke-dasharray":
                            result = result.Append(new StrokeDashArrayStyleModifier(value.Content.ToString()));
                            break;

                        case "stroke-width":
                        case "thickness":
                            if (TryParseStyleSize(value.Content.ToString(), out double thickness))
                                result = result.Append(new StrokeWidthStyleModifier(thickness));
                            break;

                        case "font-family":
                            result = result.Append(new FontFamilyStyleModifier(value.Content.ToString()));
                            break;

                        case "font-size":
                            if (TryParseStyleSize(value.Content.ToString(), out double size))
                                result = result.Append(new FontSizeStyleModifier(size));
                            break;

                        case "opacity":
                            if (double.TryParse(value.Content.ToString(), out double scale))
                                result = result.Append(new OpacityStyleModifier(scale, scale));
                            break;

                        case "foreground-opacity":
                        case "fgo":
                            if (double.TryParse(value.Content.ToString(), out scale))
                                result = result.Append(new OpacityStyleModifier(scale, null));
                            break;

                        case "background-opacity":
                        case "bgo":
                            if (double.TryParse(value.Content.ToString(), out scale))
                                result = result.Append(new OpacityStyleModifier(null, scale));
                            break;

                        case "justification":
                        case "justify":
                        case "text-align":
                            if (TryParseStyleSize(value.Content.ToString(), out double justify))
                                result = result.Append(new JustificationStyleModifier(justify));
                            break;
                    }

                    // Get rid of any semicolons
                    while (lexer.Check(Parser.Styles.TokenType.Semicolon))
                        lexer.Next();
                }
            }

            // Stroke attributes
            return result;
        }

        private IStyleModifier ParseStyleModifier(XmlNode node, ref HashSet<Marker> startMarkers, ref HashSet<Marker> endMarkers)
        {
            var style = ParseStyleModifier(node);

            // Read start markers
            string markers = node.Attributes?["marker-start"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                startMarkers ??= [];
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(startMarkers, markerToken.Content.ToString());
            }

            // Read end markers
            markers = node.Attributes?["marker-end"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                endMarkers ??= [];
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(endMarkers, markerToken.Content.ToString());
            }

            return style;
        }
        private bool TryParseStyleSize(string value, out double result)
        {
            // In pixels
            if (value.EndsWith("px"))
            {
                value = value[0..^2].Trim();
                if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result))
                {
                    result *= 3.0 / 4.0;
                    return true;
                }
                return false;
            }
            if (value.EndsWith("pt"))
            {
                value = value[0..^2].Trim();
                return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
            }
            value = value.Trim();
            return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        private void AddMarker(HashSet<Marker> markers, string value)
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
                    Diagnostics?.Post(ErrorCodes.InvalidMarker, value);
                    break;
            }
        }

        private void DrawMarkers(HashSet<Marker> markers, Vector2 location, Vector2 orientation, IStyle appearance)
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
                marker.Draw(this, appearance);
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
        public abstract IGraphicsBuilder BeginGroup(string id = null, IEnumerable<string> classes = null, bool atStart = false);

        /// <inheritdoc />
        public abstract IGraphicsBuilder EndGroup();

        /// <inheritdoc />
        public abstract IGraphicsBuilder Line(Vector2 start, Vector2 end, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Circle(Vector2 center, double radius, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Polyline(IEnumerable<Vector2> points, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Polygon(IEnumerable<Vector2> points, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, IStyle options);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Text(Span span, Vector2 location, Vector2 expand, TextOrientationType type);
    }
}
