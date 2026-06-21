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
    public MinusB()
    {
        OppositeSide = true;
    }
}