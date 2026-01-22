namespace SimpleCircuit.Components;

/// <summary>
/// Describes a component that has bounds.
/// </summary>
public interface IBoundedComponent
{
    /// <summary>
    /// Gets the left side coordinate.
    /// </summary>
    public string Left { get; }

    /// <summary>
    /// Gets the top side coordinate.
    /// </summary>
    public string Top { get; }

    /// <summary>
    /// Gets the right side coordinate.
    /// </summary>
    public string Right { get; }
    
    /// <summary>
    /// Gets the bottom side coordinate.
    /// </summary>
    public string Bottom { get; }
}
