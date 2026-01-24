namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A plus marker (opposite side).
/// </summary>
[Drawable("plusb", "A generic plus symbol (opposite side).", "General")]
public class PlusB : Plus
{
    /// <summary>
    /// Creates a new <see cref="PlusB"/> marker.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="orientation"></param>
    public PlusB(Vector2 location = default, Vector2 orientation = default)
        : base(location, orientation)
    {
        OppositeSide = true;
    }
}
