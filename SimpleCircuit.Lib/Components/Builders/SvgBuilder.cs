using SimpleCircuit.Circuits.Spans;
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
        private readonly ExpandableBounds _bounds = new();
        private readonly Stack<Transform> _tf = new();
        private readonly ITextFormatter _formatter;

        /// <inheritdoc />
        public override Bounds Bounds => _bounds.Bounds;

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
        /// <param name="diagnostics">The diagnostics handler.</param>
        public SvgBuilder(ITextFormatter formatter, IDiagnosticHandler diagnostics = null)
            : base(diagnostics)
        {
            _document = new XmlDocument();
            _current = _document.CreateElement("svg", Namespace);
            _document.AppendChild(_current);
            _tf.Push(Transform.Identity);

            // Make sure we can track the bounds of our vector image
            _formatter = formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer());
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Line(Vector2 start, Vector2 end, GraphicOptions options = null)
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
        public override IGraphicsBuilder Circle(Vector2 center, double radius, GraphicOptions options = null)
        {
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            center = CurrentTransform.Apply(center);

            // Make the circle
            var circle = _document.CreateElement("circle", Namespace);
            circle.SetAttribute("cx", center.X.ToSVG());
            circle.SetAttribute("cy", center.Y.ToSVG());
            circle.SetAttribute("r", radius.ToSVG());
            options?.Apply(circle);
            _current.AppendChild(circle);

            _bounds.Expand(
                center - new Vector2(radius, radius),
                center + new Vector2(radius, radius));
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polyline(IEnumerable<Vector2> points, GraphicOptions options = null)
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
        public override IGraphicsBuilder Polygon(IEnumerable<Vector2> points, GraphicOptions options = null)
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
        public override IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, GraphicOptions options = null)
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

        public override IGraphicsBuilder Text(Span span, Vector2 location, Vector2 expand, GraphicOptions options = null)
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
        public override IGraphicsBuilder Text(string value, Vector2 location, Vector2 expand, double size = 4.0, double lineSpacing = 1.5, GraphicOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return this;

            var span = _formatter.Format(value, size, false, options);
            return Text(span, location, expand, options);
        }

        private void BuildTextSVG(Vector2 offset, Span span, XmlNode parent, XmlElement current)
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
                        element.SetAttribute("x", (offset.X + textSpan.Offset.X).ToSVG());
                        element.SetAttribute("y", (offset.Y + textSpan.Offset.Y).ToSVG());
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
        public override IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, GraphicOptions options = null)
        {
            if (pathBuild == null)
                return this;
            var builder = new SvgPathBuilder(CurrentTransform, _bounds);
            pathBuild(builder);

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
        public override IGraphicsBuilder BeginGroup(GraphicOptions options = null, bool atStart = false)
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
