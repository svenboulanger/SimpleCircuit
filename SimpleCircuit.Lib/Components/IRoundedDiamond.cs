namespace SimpleCircuit.Components;

/// <summary>
/// Describes a rounded diamond shape.
/// </summary>
public interface IRoundedDiamond
{
    /// <summary>
    /// Gets or sets the corner radius for the left and right corners of the diamond shape.
    /// </summary>
    public double CornerRadiusX { get; set; }

    /// <summary>
    /// Gets or sets the corner radius for the top and bottom corners of the diamond shape.
    /// </summary>
    public double CornerRadiusY { get; set; }
}
