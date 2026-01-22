namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// Describes a style modifier.
/// </summary>
public interface IStyleModifier
{
    /// <summary>
    /// Applies the style modifier.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <returns>Returns the style with the modifier applied.</returns>
    public IStyle Apply(IStyle parent);
}
