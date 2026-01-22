namespace SimpleCircuit.Drawing;

/// <summary>
/// A structure for defining margins.
/// </summary>
/// <param name="left">The left margin.</param>
/// <param name="top">The top margin.</param>
/// <param name="right">The right margin.</param>
/// <param name="bottom">The bottom margin.</param>
public readonly struct Margins(double left, double top, double right, double bottom)
{
    /// <summary>
    /// Gets the left margin.
    /// </summary>
    public double Left { get; } = left;

    /// <summary>
    /// Gets the top margin.
    /// </summary>
    public double Top { get; } = top;

    /// <summary>
    /// Gets the right margin.
    /// </summary>
    public double Right { get; } = right;

    /// <summary>
    /// Gets the bottom margin.
    /// </summary>
    public double Bottom { get; } = bottom;

    /// <summary>
    /// Gets the total vertical margin.
    /// </summary>
    public double Vertical => Top + Bottom;

    /// <summary>
    /// Gets the total horizontal margin.
    /// </summary>
    public double Horizontal => Left + Right;

    /// <summary>
    /// Converts the margins to a string.
    /// </summary>
    /// <returns>The string.</returns>
    public override string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";
}
