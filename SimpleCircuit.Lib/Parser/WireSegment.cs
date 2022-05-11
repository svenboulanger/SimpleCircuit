namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public struct WireSegment
    {
        /// <summary>
        /// Gets the angle of the wire.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets whether the wire segment has a fixed length.
        /// </summary>
        public bool IsFixed { get; }

        /// <summary>
        /// Gets the length of the wire.
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Creates a new wire info.
        /// </summary>
        /// <param name="orientation">The orientation of the wire segment.</param>
        /// <param name="isFixed">If <c>true</c>, the wire has a fixed length, otherwise <paramref name="length"/> indicates a minimum length.</param>
        /// <param name="length">The length</param>
        public WireSegment(Vector2 orientation, bool isFixed, double length)
        {
            Orientation = orientation;
            IsFixed = isFixed;
            Length = length;
        }
    }
}
