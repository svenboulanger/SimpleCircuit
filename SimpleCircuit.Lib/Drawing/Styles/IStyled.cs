namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// Describes a styled item.
/// </summary>
public interface IStyled
{
    /// <summary>
    /// Gets the style.
    /// </summary>
    public IStyleModifier Modifier { get; set; }
}
