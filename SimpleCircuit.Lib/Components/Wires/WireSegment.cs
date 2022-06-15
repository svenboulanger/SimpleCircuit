namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Describes a wire segment with a start and end.
    /// </summary>
    public struct WireSegment
    {
        /// <summary>
        /// Gets the start of the wire segment.
        /// </summary>
        public Vector2 Start { get; }

        /// <summary>
        /// Gets the end of the wire segment.
        /// </summary>
        public Vector2 End { get; }

        /// <summary>
        /// Creates a new <see cref="WireSegment"/>.
        /// </summary>
        /// <param name="start">The start of the segment.</param>
        /// <param name="end">The end of the segment.</param>
        public WireSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Start} -> {End}";
    }
}
