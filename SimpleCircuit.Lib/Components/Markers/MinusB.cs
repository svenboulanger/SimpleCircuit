namespace SimpleCircuit.Components.Markers;

/// <summary>
/// A minus marker (but opposite side).
/// </summary>
[Drawable("minusb", "A generic minus symbol (opposite side).", "General")]
public class MinusB : Minus
{
    /// <summary>
    /// Creates a new <see cref="MinusB"/> marker.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public MinusB(Vector2 location = default, Vector2 orientation = default)
        : base(location, orientation)
    {
        OppositeSide = true;
    }
}