namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// Describes the appearance of an item.
/// </summary>
public interface IStyle
{
    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public string Color { get; }

    /// <summary>
    /// Gets or sets the foreground opacity.
    /// </summary>
    public double Opacity { get; }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public string Background { get; }

    /// <summary>
    /// Gets or sets the background opacity.
    /// </summary>
    public double BackgroundOpacity { get; }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public double LineThickness { get; }

    /// <summary>
    /// Gets or sets the font family.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public double FontSize { get; }

    /// <summary>
    /// Gets or sets whether text should be bold.
    /// </summary>
    public bool Bold { get; }

    /// <summary>
    /// Gets or sets the line spacing.
    /// </summary>
    public double LineSpacing { get; }

    /// <summary>
    /// Gets the text justification.
    /// </summary>
    public double Justification { get; }

    /// <summary>
    /// Gets or sets the line style.
    /// </summary>
    public string StrokeDashArray { get; }

    /// <summary>
    /// Tries to get a variable from the style.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
    public bool TryGetVariable(string key, out string value);

    /// <summary>
    /// Registers a variable to the style if it doesn't exist yet.
    /// This is used to simply guarantee that it is defined.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns <c>true</c> if the variable was added; otherwise, <c>false</c>.</returns>
    public bool RegisterVariable(string key, string value);
}
