namespace SimpleCircuit.Circuits.Contexts;

/// <summary>
/// A class for tracking extremes.
/// </summary>
public class ExtremeTracker
{
    /// <summary>
    /// Gets the minimum extreme.
    /// </summary>
    public double Minimum { get; private set; }

    /// <summary>
    /// Gets the maximum extreme.
    /// </summary>
    public double Maximum { get; private set; }

    /// <summary>
    /// Creates a new <see cref="ExtremeTracker"/>.
    /// </summary>
    public ExtremeTracker()
    {
        Minimum = 0.0;
        Maximum = 0.0;
    }

    /// <summary>
    /// Expands the extremes of the current class.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Expand(double value)
    {
        if (value < Minimum)
            Minimum = value;
        if (value > Maximum)
            Maximum = value;
    }

    /// <summary>
    /// Converts the class to a string.
    /// </summary>
    /// <returns>The string.</returns>
    public override string ToString() => $"{Minimum} ~ {Maximum}";
}
