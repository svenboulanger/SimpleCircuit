using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// A builder for graphics.
    /// </summary>
    public interface IGraphicsBuilder
    {
        /// <summary>
        /// Gets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Gets the current transform being applied.
        /// </summary>
        public Transform CurrentTransform { get; }

        /// <summary>
        /// Gets the bounds of everything rendered.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Gets a text formatter.
        /// </summary>
        public ITextFormatter TextFormatter { get; }

        /// <summary>
        /// Gets the base style.
        /// </summary>
        public IStyle Style { get; }

        /// <summary>
        /// Begins a new transform on top of previous transforms.
        /// </summary>
        /// <param name="tf">The transform.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder BeginTransform(Transform tf);

        /// <summary>
        /// Ends the last transform.
        /// </summary>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder EndTransform();

        // NOTE: There should be something better than passing a second argument for putting it at the front.
        /// <summary>
        /// Begins a new group.
        /// </summary>
        /// <param name="id">The identifier of the group.</param>
        /// <param name="classes">The classes of the group.</param>
        /// <param name="atStart">If <c>true</c>, the graphics should be added to the start of the document.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder BeginGroup(string id = null, IEnumerable<string> classes = null, bool atStart = false);

        /// <summary>
        /// Ends a group.
        /// </summary>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder EndGroup();

        /// <summary>
        /// Begins tracking a new set of bounds.
        /// </summary>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder BeginBounds();

        /// <summary>
        /// Ends tracking the bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder EndBounds(out Bounds bounds);

        /// <summary>
        /// Ensures that a given point is considered as part of the bounds.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder ExpandBounds(Vector2 point);

        /// <summary>
        /// Draws using SVG-style XML.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="context">The drawing context.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder DrawXml(XmlNode description, IXmlDrawingContext context);

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder Line(Vector2 start, Vector2 end, IStyle options);

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder Circle(Vector2 center, double radius, IStyle options);

        /// <summary>
        /// Draws a polyline.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder Polyline(IEnumerable<Vector2> points, IStyle options);

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphic builder for chaining.</returns>
        public IGraphicsBuilder Polygon(IEnumerable<Vector2> points, IStyle options);

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="rx">The radius along the X-axis.</param>
        /// <param name="ry">The radius along the Y-axis.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphic builder for chaining.</returns>
        public IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, IStyle options);

        /// <summary>
        /// Draws a path.
        /// </summary>
        /// <param name="pathBuild">The description of the path.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the graphics builder for chaining methods.</returns>
        public IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, IStyle options);

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="span">The span to be drawn.</param>
        /// <param name="location">The location where the span should be.</param>
        /// <param name="expand">The expansion direction.</param>
        /// <param name="type">The text orientation type.</param>
        /// <returns>Returns the graphics builder for chaining.</returns>
        public IGraphicsBuilder Text(Span span, Vector2 location, Vector2 expand, TextOrientationType type);
    }
}
