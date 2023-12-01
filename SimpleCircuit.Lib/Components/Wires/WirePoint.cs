namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Represents a point on a wire.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="WirePoint"/>.
    /// </remarks>
    /// <param name="location">The location of the wire point.</param>
    /// <param name="isJumpOver">If <c>true</c>, the point is an intersection point with another wire.</param>
    public readonly struct WirePoint(Vector2 location, bool isJumpOver)
    {
        /// <summary>
        /// Gets whether the point is an intersection point with another previous wire.
        /// </summary>
        public bool IsJumpOver { get; } = isJumpOver;

        /// <summary>
        /// Gets the location of the point.
        /// </summary>
        public Vector2 Location { get; } = location;

        /// <inheritdoc />
        public override string ToString() => Location.ToString();
    }
}
