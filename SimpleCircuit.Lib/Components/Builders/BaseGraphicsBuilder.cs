﻿using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser.Markers;
using SimpleCircuit.Parser.SvgPathData;
using SimpleCircuit.Parser.Variants;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Components.Builders
{
    /// <summary>
    /// A base <see cref="IGraphicsBuilder"/>.
    /// </summary>
    public abstract class BaseGraphicsBuilder : IGraphicsBuilder
    {
        private readonly Stack<Transform> _tf = new();

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; }

        /// <inheritdoc />
        public ISet<string> RequiredCSS { get; } = new HashSet<string>();

        /// <summary>
        /// Adds extra CSS after the required CSS (ordered).
        /// </summary>
        public IList<string> ExtraCSS { get; } = [];

        /// <inheritdoc />
        public Transform CurrentTransform => _tf.Peek();

        /// <inheritdoc />
        public abstract Bounds Bounds { get; }

        /// <summary>
        /// Creates a new <see cref="BaseGraphicsBuilder"/>.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        protected BaseGraphicsBuilder(IDiagnosticHandler diagnostics)
        {
            Diagnostics = diagnostics;
            _tf.Push(Transform.Identity);
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
        public abstract IGraphicsBuilder BeginGroup(GraphicOptions options = null, bool atStart = false);

        /// <inheritdoc />
        public abstract IGraphicsBuilder EndGroup();

        /// <inheritdoc />
        public abstract IGraphicsBuilder Line(Vector2 start, Vector2 end, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Circle(Vector2 center, double radius, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Polyline(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Polygon(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Text(Span span, Vector2 location, Vector2 expand, GraphicOptions options = null);

        /// <inheritdoc />
        public abstract IGraphicsBuilder Text(string value, Vector2 location, Vector2 expand, double size = 4, double lineSpacing = 1.5, GraphicOptions options = null);
    }
}