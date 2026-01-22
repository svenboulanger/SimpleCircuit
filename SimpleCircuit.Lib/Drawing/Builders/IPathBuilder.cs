namespace SimpleCircuit.Drawing.Builders;

/// <summary>
/// An interface that describes how to build a path.
/// </summary>
public interface IPathBuilder
{
    /// <summary>
    /// Gets the starting point of the path builder.
    /// </summary>
    /// <remarks>Can be used by markers.</remarks>
    public Vector2 Start { get; }

    /// <summary>
    /// Gets the starting normal of the path builder.
    /// </summary>
    /// <remarks>Can be used by markers.</remarks>
    public Vector2 StartNormal { get; }

    /// <summary>
    /// Gets the ending point of the path builder.
    /// </summary>
    /// <remarks>Can be used by markers.</remarks>
    public Vector2 End { get; }

    /// <summary>
    /// Gets the ending normal of the path builder.
    /// </summary>
    /// <remarks>Can be used by markers.</remarks>
    public Vector2 EndNormal { get; }

    /// <summary>
    /// Moves the current point using absolute coordinates.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder MoveTo(Vector2 location);

    /// <summary>
    /// Moves the current point using relative coordinates.
    /// </summary>
    /// <param name="delta">The step.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Move(Vector2 delta);

    /// <summary>
    /// Draws a line using absolute coordinates.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder LineTo(Vector2 location);

    /// <summary>
    /// Draws a line using absolute coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Line(Vector2 delta);

    /// <summary>
    /// Draws a horizontal line using absolute coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder HorizontalTo(double x);

    /// <summary>
    /// Draws a horizontal line using relative coordinates.
    /// </summary>
    /// <param name="dx">The step.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Horizontal(double dx);

    /// <summary>
    /// Draws a vertical line using absolute coordinates.
    /// </summary>
    /// <param name="y">The x-coordinate.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder VerticalTo(double y);

    /// <summary>
    /// Draws a vertical line using relative coordinates.
    /// </summary>
    /// <param name="dy">The step.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Vertical(double dy);

    /// <summary>
    /// Draws a bezier curve using absolute coordinates.
    /// </summary>
    /// <param name="h1">The first handle.</param>
    /// <param name="h2">The second handle.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder CurveTo(Vector2 h1, Vector2 h2, Vector2 end);

    /// <summary>
    /// Draws a bezier curve using relative coordinates.
    /// </summary>
    /// <param name="dh1">The first handle.</param>
    /// <param name="dh2">The second handle.</param>
    /// <param name="dend">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Curve(Vector2 dh1, Vector2 dh2, Vector2 dend);

    /// <summary>
    /// Draws a smooth bezier curve using absolute coordinates.
    /// </summary>
    /// <param name="h">The handle.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder SmoothTo(Vector2 h, Vector2 end);

    /// <summary>
    /// Draws a smooth bezier curve using relative coordinates.
    /// </summary>
    /// <param name="dh">The handle.</param>
    /// <param name="dend">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Smooth(Vector2 dh, Vector2 dend);

    /// <summary>
    /// Draws a quadrature bezier curve using absolute coordinates.
    /// </summary>
    /// <param name="h">The handle.</param>
    /// <param name="end">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder QuadCurveTo(Vector2 h, Vector2 end);

    /// <summary>
    /// Draws a quadrature bezier curve using relative coordinates.
    /// </summary>
    /// <param name="dh">The handle.</param>
    /// <param name="dend">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder QuadCurve(Vector2 dh, Vector2 dend);

    /// <summary>
    /// Draws a smooth quadrature bezier curve using absolute coordinates.
    /// </summary>
    /// <param name="end">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder SmoothQuadTo(Vector2 end);

    /// <summary>
    /// Draws a smooth quadrature bezier curve using relative coordinates.
    /// </summary>
    /// <param name="dend">The end point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder SmoothQuad(Vector2 dend);

    /// <summary>
    /// Draws an arc.
    /// </summary>
    /// <param name="rx">The radius in X-direction.</param>
    /// <param name="ry">The radius in Y-direction.</param>
    /// <param name="angle">The angle of the X-axis.</param>
    /// <param name="largeArc">The large-arc argument. If <c>true</c>, the arc that is greater than 180deg is chosen.</param>
    /// <param name="sweepFlag">The sweep direction. If <c>true</c>, the sweep is through increasing angles.</param>
    /// <param name="end">The end point of the arc.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder ArcTo(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 end);

    /// <summary>
    /// Draws an arc.
    /// </summary>
    /// <param name="rx">The radius in X-direction.</param>
    /// <param name="ry">The radius in Y-direction.</param>
    /// <param name="angle">The angle of the X-axis.</param>
    /// <param name="largeArc">The large-arc argument. If <c>true</c>, the arc that is greater than 180deg is chosen.</param>
    /// <param name="sweepFlag">The sweep direction. If <c>true</c>, the sweep is through increasing angles.</param>
    /// <param name="dend">The end point of the arc relative to the current point.</param>
    /// <returns>The path builder.</returns>
    public IPathBuilder Arc(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 dend);

    /// <summary>
    /// Closes the path.
    /// </summary>
    /// <returns>The path builder.</returns>
    public IPathBuilder Close();
}
