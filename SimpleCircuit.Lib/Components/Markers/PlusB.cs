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
    public PlusB()
    {
        OppositeSide = true;
    }
}
