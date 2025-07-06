using SimpleCircuit.Components;
using SimpleCircuit.Components.General;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser.Markers;
using SimpleCircuit.Parser.Styles;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Parser.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

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
            // Calculate the style for the following actions
            var style = context.Modifier?.Apply(Style) ?? Style;

            foreach (XmlNode node in parent.ChildNodes)
            {
                // If there is an attribute called "variant", evaluate whether this action should be processed
                if (!EvaluateVariants(node.Attributes?["variant"]?.Value, context))
                    continue;

                // Depending on the node type, let's draw something!
                switch (node.Name)
                {
                    case "#comment": break;
                    case "line": DrawXmlLine(node, style); break;
                    case "circle": DrawXmlCircle(node, style); break;
                    case "path": DrawXmlPath(node, style); break;
                    case "polygon": DrawXmlPolygon(node, style); break;
                    case "polyline": DrawXmlPolyline(node, style); break;
                    case "rect": DrawXmlRectangle(node, style); break;
                    case "text": DrawXmlText(node, style); break;
                    case "variant":
                    case "v": DrawXmlActions(node, context); break;
                    case "select":
                    case "switch":
                    case "s": DrawXmlSelect(node, context); break;
                    case "label": DrawXmlLabelAnchor(node, context); break;
                    case "group":
                    case "g":
                        // Parse style options
                        var modifier = ParseStyleModifier(node, Diagnostics);
                        var oldModifier = context.Modifier;
                        context.Modifier = context.Modifier.Append(modifier);

                        // Recursively descend
                        BeginGroup();
                        DrawXmlActions(node, context);
                        EndGroup();

                        // Rettore
                        context.Modifier = oldModifier;
                        break;
                    default:
                        Diagnostics?.Post(ErrorCodes.CouldNotRecognizeDrawingCommand, node.Name);
                        break;
                }
            }
        }

        private void DrawXmlLine(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            // Parse main properties
            bool success = true;
            success &= node.Attributes.ParseOptionalVector("x1", "y1", Diagnostics, Vector2.Zero, out var start);
            success &= node.Attributes.ParseOptionalVector("x2", "y2", Diagnostics, Vector2.Zero, out var end);
            if (!success)
                return;

            // Parse the style
            var modifier = ParseStyleModifier(node, Diagnostics, out var startMarkers, out var endMarkers);
            style = modifier?.Apply(style) ?? style;

            // Draw
            Line(start, end, style);
            DrawMarkers(startMarkers, start, start - end, style);
            DrawMarkers(endMarkers, end, end - start, style);
        }

        private void DrawXmlPolygon(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            // Parse points
            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr);
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;

            // Parse the style
            var modifier = ParseStyleModifier(node, Diagnostics);
            style = modifier?.Apply(style) ?? style;

            // Draw the polygon
            Polygon(points, style);
        }

        private void DrawXmlPolyline(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            // Parse the points
            string attr = node.Attributes?["points"]?.Value;
            if (attr == null)
                return;
            var lexer = new SvgPathDataLexer(attr);
            var points = SvgPathDataParser.ParsePoints(lexer, Diagnostics);
            if (points == null || points.Count <= 1)
                return;

            // Parse the style
            var modifier = ParseStyleModifier(node, Diagnostics, out var startMarkers, out var endMarkers);
            style = modifier?.Apply(style) ?? style;

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

        private void DrawXmlCircle(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalVector("cx", "cy", Diagnostics, Vector2.Zero, out var center);
            success &= node.Attributes.ParseOptionalScalar("r", Diagnostics, 0.0, out double r);
            if (!success)
                return;

            var modifier = ParseStyleModifier(node, Diagnostics);
            style = modifier?.Apply(style) ?? style;

            // Draw the circle
            Circle(center, r, style);
        }

        private void DrawXmlPath(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            // Parse the style
            var modifier = ParseStyleModifier(node, Diagnostics, out var startMarkers, out var endMarkers);
            style = modifier?.Apply(style) ?? style;

            // Get the path data
            string pathData = node.Attributes?["d"]?.Value;
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

        private void DrawXmlRectangle(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            // Parse main properties
            bool success = true;
            success &= node.Attributes.ParseOptionalVector("x", "y", Diagnostics, Vector2.Zero, out var location);
            success &= node.Attributes.ParseOptionalVector("width", "height", Diagnostics, Vector2.Zero, out var size);
            success &= node.Attributes.ParseOptionalVector("rx", "ry", Diagnostics, Vector2.NaN, out var radius);
            if (!success)
                return;

            // Parse the style
            var modifier = ParseStyleModifier(node, Diagnostics);
            style = modifier?.Apply(style) ?? style;

            // Draw the rectangle
            this.Rectangle(location.X, location.Y, size.X, size.Y, style, radius.X, radius.Y);
        }

        private void DrawXmlText(XmlNode node, IStyle style)
        {
            if (node.Attributes == null)
                return;

            bool success = true;
            success &= node.Attributes.ParseOptionalVector("x", "y", Diagnostics, Vector2.Zero, out var location);
            success &= node.Attributes.ParseOptionalVector("nx", "ny", Diagnostics, Vector2.UX, out var orientation);
            success &= node.Attributes.ParseOptionalVector("ex", "ey", Diagnostics, Vector2.NaN, out var expand);
            if (!success)
                return;

            // Normalize the vectors
            if (!expand.IsNaN())
                expand /= expand.Length;
            orientation /= orientation.Length;

            // Get the style
            var modifier = ParseStyleModifier(node, Diagnostics, asText: true);
            style = modifier?.Apply(style) ?? style;

            // Get the text value
            string value = node.Attributes?["value"]?.Value;
            if (value is null)
                return;
            Label label = new() { Value = value };
            label.Format(TextFormatter, style);

            // Get how the text should be placed
            if (!TryGetTextOrientationType(node, out var type))
                return;
            if (!TryGetAnchor(node, out var textAnchor))
                return;

            // Draw the text
            var anchorPoint = new LabelAnchorPoint(location, expand, orientation, type, textAnchor);
            LabelAnchorPoints<IDrawable>.DrawLabel(this, anchorPoint, [label]);
        }

        private void DrawXmlLabelAnchor(XmlNode node, IXmlDrawingContext context)
        {
            // Get the main properties
            bool success = true;
            success &= node.Attributes.ParseOptionalVector("x", "y", Diagnostics, default, out var location);
            success &= node.Attributes.ParseOptionalVector("nx", "ny", Diagnostics, Vector2.UX, out var orientation);
            success &= node.Attributes.ParseOptionalVector("ex", "ey", Diagnostics, Vector2.NaN, out var expand);
            if (!success)
                return;
            if (!expand.IsNaN())
                expand /= expand.Length;
            orientation /= orientation.Length;

            // Get the text orientation type
            if (!TryGetTextOrientationType(node, out var type))
                return;

            // Get the text anchor
            if (!TryGetAnchor(node, out var textAnchor))
                return;

            context.Anchors.Add(new LabelAnchorPoint(location, expand, orientation, type, textAnchor));
        }
        private bool TryGetTextOrientationType(XmlNode node, out TextOrientationType type)
        {
            // Now let's transform the text location and orientation if necessary
            string mode = node.Attributes?["mode"]?.Value;
            if (mode is not null)
            {
                switch (mode.ToLower())
                {
                    case "transformed": type = TextOrientationType.Transformed; return true;
                    case "none": type = TextOrientationType.None; return true;
                    default:
                        Diagnostics?.Post(ErrorCodes.InvalidTextOrientationType, mode);
                        type = TextOrientationType.Transformed;
                        return false;
                }
                ;
            }
            type = TextOrientationType.Transformed;
            return true;
        }
        private bool TryGetAnchor(XmlNode node, out TextAnchor anchor)
        {
            string a = node.Attributes?["anchor"]?.Value;
            if (a is not null)
            {
                switch (a.ToLower())
                {
                    case "center": anchor = TextAnchor.Center; return true;

                    case "left":
                    case "left-middle":
                    case "leftmiddle":
                    case "middle-left":
                    case "middleleft": anchor = TextAnchor.MiddleLeft; return true;

                    case "left-top":
                    case "lefttop":
                    case "top-left":
                    case "topleft": anchor = TextAnchor.TopLeft; return true;

                    case "top":
                    case "center-top":
                    case "centertop":
                    case "top-center":
                    case "topcenter": anchor = TextAnchor.TopCenter; return true;

                    case "top-right":
                    case "topright":
                    case "right-top":
                    case "righttop": anchor = TextAnchor.TopRight; return true;

                    case "right":
                    case "right-middle":
                    case "rightmiddle":
                    case "middle-right":
                    case "middleright": anchor = TextAnchor.MiddleRight; return true;

                    case "bottom-right":
                    case "bottomright":
                    case "right-bottom":
                    case "rightbottom": anchor = TextAnchor.BottomRight; return true;

                    case "bottom":
                    case "center-bottom":
                    case "centerbottom":
                    case "bottom-center":
                    case "bottomcenter": anchor = TextAnchor.BottomCenter; return true;

                    case "bottom-left":
                    case "bottomleft":
                    case "left-bottom":
                    case "leftbottom": anchor = TextAnchor.BottomLeft; return true;

                    case "origin":
                    case "begin": // This is for SVG text-align compatibility
                    case "begin-baseline":
                    case "beginbaseline":
                    case "baseline-begin":
                    case "baselinebegin": anchor = TextAnchor.Origin; return true;

                    case "middle": // This is just for SVG text-align compatibility
                    case "center-baseline":
                    case "centerbaseline":
                    case "baseline-center":
                    case "baselinecenter": anchor = TextAnchor.BaselineCenter; return true;

                    case "end": // This is for SVG text-align compatibility
                    case "end-baseline":
                    case "endbaseline":
                    case "baseline-end":
                    case "baselineend": anchor = TextAnchor.BaselineEnd; return true;

                    case "begin-middle":
                    case "beginmiddle":
                    case "middle-begin":
                    case "middlebegin": anchor = TextAnchor.MiddleBegin; return true;

                    case "end-middle":
                    case "endmiddle":
                    case "middle-end":
                    case "middleend": anchor = TextAnchor.MiddleEnd; return true;

                    case "begin-top":
                    case "begintop":
                    case "top-begin":
                    case "topbegin": anchor = TextAnchor.TopBegin; return true;

                    case "end-top":
                    case "endtop":
                    case "topend":
                    case "top-end": anchor = TextAnchor.TopEnd; return true;

                    case "begin-bottom":
                    case "beginbottom":
                    case "bottom-begin":
                    case "bottombegin": anchor = TextAnchor.BottomBegin; return true;

                    case "end-bottom":
                    case "endbottom":
                    case "bottom-end":
                    case "bottomend": anchor = TextAnchor.BottomEnd; return true;

                    default:
                        Diagnostics?.Post(ErrorCodes.CouldNotRecognizeBracket, a);
                        anchor = TextAnchor.Origin;
                        return false;
                }
            }
            anchor = TextAnchor.Origin;
            return true;
        }
        
        /// <summary>
        /// Parses a style attribute, or attributes that relate to the style of an XML node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="asText">If <c>true</c>, the fill is treated as the foreground color to allow SVG syntax.</param>
        /// <returns>Returns the style modifier.</returns>
        public static IStyleModifier ParseStyleModifier(XmlNode node, IDiagnosticHandler diagnostics, bool asText = false)
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

        /// <summary>
        /// Parses a style attribute, or attributes that relate to the style of an XML node. It also parses
        /// start- and end-markers.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <param name="startMarkers">The set of markers that should be at the start.</param>
        /// <param name="endMarkers">The set of markers that should be at the end.</param>
        /// <returns>Returns the style modifier.</returns>
        public IStyleModifier ParseStyleModifier(XmlNode node, IDiagnosticHandler diagnostics, out HashSet<Marker> startMarkers, out HashSet<Marker> endMarkers)
        {
            var style = ParseStyleModifier(node, diagnostics);

            // Read start markers
            string markers = node.Attributes?["marker-start"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                startMarkers = [];
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(startMarkers, markerToken.Content.ToString());
            }
            else
                startMarkers = null;

            // Read end markers
            markers = node.Attributes?["marker-end"]?.Value;
            if (!string.IsNullOrWhiteSpace(markers))
            {
                endMarkers = [];
                var lexer = new MarkerLexer(markers);
                while (lexer.Branch(Parser.Markers.TokenType.Marker, out var markerToken))
                    AddMarker(endMarkers, markerToken.Content.ToString());
            }
            else
                endMarkers = null;

            return style;
        }
        private static bool TryParseStyleSize(string value, out double result)
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

        private void DrawXmlSelect(XmlNode node, IXmlDrawingContext context)
        {
            // We will construct a number of nodes of which we can only select one
            Dictionary<string, List<XmlNode>> map = [];
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "case")
                {
                    // Get the variant name
                    string variant = "default";
                    if (child.Attributes is not null)
                        variant = child.Attributes["variant"]?.Value ?? child.Attributes["v"]?.Value ?? "default";
                    
                    if (!map.TryGetValue(variant, out var list))
                    {
                        list = [];
                        map.Add(variant, list);
                    }
                    list.Add(child);
                }
                else
                {
                    Diagnostics?.Post(ErrorCodes.CouldNotRecognizeDrawingCommand, node.Name);
                    return;
                }
            }

            // Now do the selection
            string[] keys = [.. map.Where(p => p.Key != "default").Select(p => p.Key)];
            int index = context.Variants.Select(keys);
            string key = index < 0 ? "default" : keys[index];
            if (map.TryGetValue(key, out var l))
            {
                foreach (var n in l)
                    DrawXmlActions(n, context);
            }
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
        public abstract IGraphicsBuilder Text(Span span, Vector2 location, Vector2 orientation, TextOrientationType type);
    }
}
