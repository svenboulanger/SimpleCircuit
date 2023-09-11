namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Represents a point on a wire.
    /// </summary>
    public readonly struct WirePoint
    {
        /// <summary>
        /// Gets whether the point is an intersection point with another previous wire.
        /// </summary>
        public bool IsJumpOver { get; }

        /// <summary>
        /// Gets the location of the point.
        /// </summary>
        public Vector2 Location { get; }

        /// <summary>
        /// Creates a new <see cref="WirePoint"/>.
        /// </summary>
        /// <param name="location">The location of the wire point.</param>
        /// <param name="isJumpOver">If <c>true</c>, the point is an intersection point with another wire.</param>
        public WirePoint(Vector2 location, bool isJumpOver)
        {
            Location = location;
            IsJumpOver = isJumpOver;
        }

        /// <inheritdoc />
        public override string ToString() => Location.ToString();
    }
}
