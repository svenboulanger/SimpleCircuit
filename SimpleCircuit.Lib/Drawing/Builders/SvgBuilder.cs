using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// An <see cref="IGraphicsBuilder"/> for making SVG format data.
    /// </summary>
    public class SvgBuilder : BaseGraphicsBuilder
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
        private readonly Stack<Transform> _tf = new();

        /// <summary>
        /// Gets or sets the margin used along the border to make sure everything is included.
        /// </summary>
        public Margins Margin { get; set; } = new(2, 2, 2, 2);

        /// <summary>
        /// Removes empty groups.
        /// </summary>
        public bool RemoveEmptyGroups { get; set; } = true;

        /// <summary>
        /// Creates a new SVG drawing instance.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public SvgBuilder(ITextFormatter formatter, IStyle style, IDiagnosticHandler diagnostics)
            : base(formatter, style, diagnostics)
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Line(Vector2 start, Vector2 end, IStyle options)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);

            // Create the line
            var line = _document.CreateElement("line", Namespace);
            line.SetAttribute("x1", start.X.ToSVG());
            line.SetAttribute("y1", start.Y.ToSVG());
            line.SetAttribute("x2", end.X.ToSVG());
            line.SetAttribute("y2", end.Y.ToSVG());
            line.SetAttribute("style", options.CreateStrokeStyle(Diagnostics));
            _current.AppendChild(line);

            Expand(start, end);
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Circle(Vector2 center, double radius, IStyle options)
        {
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            center = CurrentTransform.Apply(center);

            // Make the circle
            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", center.X.ToSVG());
            circle.SetAttribute("cy", center.Y.ToSVG());
            circle.SetAttribute("r", radius.ToSVG());
            circle.SetAttribute("style", options.CreateStrokeFillStyle(Diagnostics));
            _current.AppendChild(circle);

            Expand(
                center - new Vector2(radius, radius),
                center + new Vector2(radius, radius));
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polyline(IEnumerable<Vector2> points, IStyle options)
        {
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(tpt.ToSVG());
            }

            // Creates the poly
            var poly = _document.CreateElement("polyline", Namespace);
            poly.SetAttribute("style", options.CreateStrokeStyle(Diagnostics));
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polygon(IEnumerable<Vector2> points, IStyle options)
        {
            StringBuilder sb = new();
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(tpt.ToSVG());
            }

            // Create the element
            var poly = _document.CreateElement("polygon", Namespace);
            _current.AppendChild(poly);
            poly.SetAttribute("points", sb.ToString());
            poly.SetAttribute("style", options.CreateStrokeFillStyle(Diagnostics));
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, IStyle options)
        {
            double kx = rx * 0.552284749831;
            double ky = ry * 0.552284749831;
            BeginTransform(new Transform(center, Matrix2.Identity));
            Path(b => b.MoveTo(new(-rx, 0))
                .CurveTo(new(-rx, -ky), new(-kx, -ry), new(0, -ry))
                .CurveTo(new(kx, -ry), new(rx, -ky), new(rx, 0))
                .CurveTo(new(rx, ky), new(kx, ry), new(0, ry))
                .CurveTo(new(-kx, ry), new(-rx, ky), new(-rx, 0)).Close(),
                options);
            EndTransform();
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Text(Span span, Vector2 location, Vector2 orientation, TextOrientationType type)
        {
            if (span is null)
                return this;

            // Create the group
            var g = _document.CreateElement("g", Namespace);
            _current.AppendChild(g);

            location = CurrentTransform.Apply(location);
            var bounds = span.Bounds.Bounds;

            // First determine the orientation
            if ((type & TextOrientationType.Transformed) != 0)
            {
                orientation = CurrentTransform.ApplyDirection(orientation);
                if (!orientation.IsZero())
                    orientation /= orientation.Length;

                if (orientation.X < 0)
                {
                    // Orientation is towards the left, so this will cause the text to be upside-down
                    // We will flip the orientation and make the text location start from the other end
                    if (CurrentTransform.Matrix.Determinant < 0)
                        location += orientation * (bounds.Right + bounds.Left);
                    else
                        location += orientation * (bounds.Right + bounds.Left) + orientation.Perpendicular * (bounds.Top + bounds.Bottom);
                    orientation = -orientation;
                }
                else if (CurrentTransform.Matrix.Determinant < 0)
                    location -= orientation.Perpendicular * (bounds.Top + bounds.Bottom);
            }

            // Expand bounds
            foreach (var p in span.Bounds.Bounds)
                Expand(location + p.X * orientation + p.Y * orientation.Perpendicular);

            // Apply orientation and location to the containing group
            double angle = Math.Atan2(orientation.Y, orientation.X) / Math.PI * 180.0;
            if (angle.IsZero())
                g.SetAttribute("transform", $"translate({location.ToSVG()})");
            else
                g.SetAttribute("transform", $"translate({location.ToSVG()}) rotate({angle.ToSVG()})");

            // Create the text element
            var text = _document.CreateElement("text", Namespace);
            g.AppendChild(text);

            // Make the SVG for the text
            BuildTextSVG(span, g, text);
            return this;
        }

        private void BuildTextSVG(Span span, XmlNode groupElement, XmlElement textElement)
        {
            if (span is null)
                return;
            switch (span)
            {
                case TextSpan textSpan:
                    {
                        // Make a span at the specified location
                        var element = _document.CreateElement("tspan", Namespace);
                        element.SetAttribute("style", textSpan.Appearance.CreateTextStyle(Diagnostics));
                        element.SetAttribute("x", textSpan.Offset.X.ToSVG());
                        element.SetAttribute("y", textSpan.Offset.Y.ToSVG());
                        element.InnerXml = textSpan.Content;
                        textElement.AppendChild(element);

                        /*
                        // Add bounds
                        var path = _document.CreateElement("path", Namespace);
                        groupElement.AppendChild(path);
                        var sb = new StringBuilder();
                        sb.Append("M");
                        sb.Append((textSpan.Offset + textSpan.Bounds.Bounds.TopLeft).ToSVG()); sb.Append(' ');
                        sb.Append((textSpan.Offset + textSpan.Bounds.Bounds.TopRight).ToSVG()); sb.Append(' ');
                        sb.Append((textSpan.Offset + textSpan.Bounds.Bounds.BottomRight).ToSVG()); sb.Append(' ');
                        sb.Append((textSpan.Offset + textSpan.Bounds.Bounds.BottomLeft).ToSVG());
                        sb.Append('Z');
                        path.SetAttribute("d", sb.ToString());
                        path.SetAttribute("style", "stroke-width: 0.1pt; stroke: red; fill: none;");
                        */
                    }
                    break;

                case LineSpan lineSpan:
                    {
                        foreach (var s in lineSpan)
                            BuildTextSVG(s, groupElement, textElement);
                    }
                    break;

                case MultilineSpan multilineSpan:
                    {
                        foreach (var s in multilineSpan)
                            BuildTextSVG(s, groupElement, textElement);
                    }
                    break;

                case SubscriptSuperscriptSpan subSuperSpan:
                    {
                        BuildTextSVG(subSuperSpan.Base, groupElement, textElement);
                        BuildTextSVG(subSuperSpan.Sub, groupElement, textElement);
                        BuildTextSVG(subSuperSpan.Super, groupElement, textElement);
                    }
                    break;

                case OverlineSpan overlineSpan:
                    {
                        BuildTextSVG(overlineSpan.Base, groupElement, textElement);

                        // Make the line manually as it bypasses the transform step
                        var path = _document.CreateElement("path", Namespace);
                        groupElement.AppendChild(path);
                        path.SetAttribute("d", $"M{overlineSpan.Start.ToSVG()} {overlineSpan.End.ToSVG()}");
                        path.SetAttribute("style", overlineSpan.Style.CreateStrokeStyle(Diagnostics));
                    }
                    break;

                case UnderlineSpan underlineSpan:
                    {
                        BuildTextSVG(underlineSpan.Base, groupElement, textElement);

                        // Make the line manually as it bypasses the transform step
                        var path = _document.CreateElement("path", Namespace);
                        groupElement.AppendChild(path);
                        path.SetAttribute("d", $"M{underlineSpan.Start.ToSVG()} {underlineSpan.End.ToSVG()}");
                        path.SetAttribute("style", underlineSpan.Style.CreateStrokeStyle(Diagnostics));
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, IStyle options)
        {
            if (pathBuild == null)
                return this;
            var bounds = new ExpandableBounds();
            var builder = new SvgPathBuilder(CurrentTransform, bounds);
            pathBuild(builder);
            Expand(bounds.Bounds);

            // Create the path element
            var path = _document.CreateElement("path", Namespace);
            _current.AppendChild(path);
            path.SetAttribute("d", builder.ToString());
            path.SetAttribute("style", options.CreateStrokeFillStyle(Diagnostics));
            return this;
        }

        /// <summary>
        /// Begins a new group.
        /// </summary>
        /// <param name="options">The options.</param>
        public override IGraphicsBuilder BeginGroup(string id = null, IEnumerable<string> classes = null, bool atStart = false)
        {
            var elt = _document.CreateElement("g", Namespace);
            if (atStart)
                _current.PrependChild(elt);
            else
                _current.AppendChild(elt);
            if (!string.IsNullOrWhiteSpace(id))
                elt.SetAttribute("id", id);
            if (classes is not null)
                elt.SetAttribute("class", string.Join(" ", classes));
            _current = elt;
            return this;
        }

        /// <summary>
        /// Ends the last opened group.
        /// </summary>
        public override IGraphicsBuilder EndGroup()
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

            // Try to get the bounds of this
            var bounds = Bounds.Expand(Margin);

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
