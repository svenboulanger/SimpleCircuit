using SimpleCircuit.Components.Builders;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Describes an instance that is able to draw.
    /// </summary>
    public interface IDrawing
    {
        /// <summary>
        /// Gets the current transform.
        /// </summary>
        public Transform CurrentTransform { get; }

        /// <summary>
        /// Gets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Begins a new transform on top of the current transform.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public void BeginTransform(Transform transform);

        /// <summary>
        /// Ends the last transform.
        /// </summary>
        public void EndTransform();

        /// <summary>
        /// Begins a new group.
        /// </summary>
        /// <param name="options"></param>
        public void BeginGroup(GraphicOptions options);

        /// <summary>
        /// Ends the last group.
        /// </summary>
        /// <returns>The bounds of the group using absolute coordinates.</returns>
        public Bounds EndGroup();

        /// <summary>
        /// Makes sure the given point is included in the drawing bounds.
        /// </summary>
        /// <param name="point">The point.</param>
        public void Expand(Vector2 point);

        /// <summary>
        /// Draws a vector drawing from XML nodes.
        /// </summary>
        /// <param name="description">The description of the drawing.</param>
        /// <returns>The bounds of the drawing described.</returns>
        public Bounds DrawXml(XmlNode description);

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start point as local coordinates.</param>
        /// <param name="end">The end point as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the line in absolute coordinates.</returns>
        public Bounds Line(Vector2 start, Vector2 end, GraphicOptions options = null);

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">The center of the circle as local coordinates.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the circle in absolute coordinates.</returns>
        public Bounds Circle(Vector2 center, double radius, GraphicOptions options = null);

        /// <summary>
        /// Draws a polyline (connected lines).
        /// </summary>
        /// <param name="points">The points as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the polyline using absolute coordinates.</returns>
        public Bounds Polyline(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <summary>
        /// Draws a polygon (a closed shape).
        /// </summary>
        /// <param name="points">The points as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the polygon using absolute coordinates.</returns>
        public Bounds Polygon(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <summary>
        /// Draws a smooth bezier curve.
        /// </summary>
        /// <param name="points">The points as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the bezier curve using absolute coordinate.</returns>
        public Bounds SmoothBezier(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <summary>
        /// Draws a closed bezier curve.
        /// </summary>
        /// <param name="points">The points as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the bezier curve using absolute coordinates.</returns>
        public Bounds ClosedBezier(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <summary>
        /// Draws an open bezier curve.
        /// </summary>
        /// <param name="points">The points as local coordinates.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the bezier curve as global coordinates.</returns>
        public Bounds OpenBezier(IEnumerable<Vector2> points, GraphicOptions options = null);

        /// <summary>
        /// Draw an arc.
        /// </summary>
        /// <param name="center">The center of the arc.</param>
        /// <param name="startAngle">The starting angle of the arc.</param>
        /// <param name="endAngle">The end angle of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="options">The graphic options.</param>
        /// <param name="intermediatePoints">The number of intermediate points; if <c>0</c>, it is determined automatically.</param>
        /// <returns>The bounds of the arc as global coordinates.</returns>
        public Bounds Arc(Vector2 center, double startAngle, double endAngle, double radius, GraphicOptions options = null, int intermediatePoints = 0);

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse as local coordinates.</param>
        /// <param name="rx">The radius along the local x-axis.</param>
        /// <param name="ry">The radius along the local y-axis.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the ellipse as global coordinates.</returns>
        public Bounds Ellipse(Vector2 center, double rx, double ry, GraphicOptions options = null);

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <param name="location">The location of the text.</param>
        /// <param name="expand">The quadrant in which the text should overflow.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the text as global coordinates.</returns>
        public Bounds Text(string value, Vector2 location, Vector2 expand, GraphicOptions options = null);

        /// <summary>
        /// Draws a path.
        /// </summary>
        /// <param name="builder">The path builder.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>The bounds of the path as global coordinates.</returns>
        public Bounds Path(Action<SvgPathBuilder> builder, GraphicOptions options = null);
    }
}
