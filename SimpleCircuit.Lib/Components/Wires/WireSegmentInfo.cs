namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public struct WireSegmentInfo
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
        /// Gets the label for the wire segment.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets whether the label should be flipping sides.
        /// </summary>
        public bool Flipped { get; }

        /// <summary>
        /// Creates a new wire info.
        /// </summary>
        /// <param name="orientation">The orientation of the wire segment.</param>
        /// <param name="isFixed">If <c>true</c>, the wire has a fixed length, otherwise <paramref name="length"/> indicates a minimum length.</param>
        /// <param name="length">The length</param>
        /// <param name="label">The label for the wire segment.</param>
        /// <param name="flipped">If <c>true</c>, the label appears on the other side.</param>
        public WireSegmentInfo(Vector2 orientation, bool isFixed, double length, string label, bool flipped)
        {
            Orientation = orientation;
            IsFixed = isFixed;
            Length = length;
            Label = label;
            Flipped = flipped;
        }
    }
}
