namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public class WireSegmentInfo
    {
        /// <summary>
        /// Gets the angle of the wire.
        /// </summary>
        public Vector2 Orientation { get; set; }

        /// <summary>
        /// Gets whether the wire segment has a fixed length.
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// Gets the length of the wire.
        /// </summary>
        public double Length { get; set; }
    }
}
