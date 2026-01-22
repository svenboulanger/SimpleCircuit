namespace SimpleCircuit.Components;

/// <summary>
/// A drawable that can transform (rotate, scale, etc.), instead of just translating.
/// </summary>
public interface ITransformingDrawable : ILocatedDrawable
{
    /// <summary>
    /// Gets the degrees of freedom for the orientation of the drawable.
    /// </summary>
    /// <remarks>
    ///     If 0, the drawable is completely constrained. If 1, the drawable
    ///     can still be flipped. If 2, the drawable can still be rotated and
    ///     flipped.
    /// </remarks>
    public int OrientationDegreesOfFreedom { get; }

    /// <summary>
    /// Transforms an offset relative to this drawable.
    /// </summary>
    /// <param name="local">The local offset.</param>
    /// <returns>The transformed offset vector.</returns>
    public Vector2 TransformOffset(Vector2 local);

    /// <summary>
    /// Transforms a normal, representing a direction, relative to this drawable.
    /// </summary>
    /// <param name="local">The local offset.</param>
    /// <returns>The transformed normal vector.</returns>
    public Vector2 TransformNormal(Vector2 local);
}
