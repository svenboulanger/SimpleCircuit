namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public struct WireInfo
    {
        /// <summary>
        /// Gets the angle of the wire.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets the length of the wire.
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Gets the minimum length of the wire.
        /// </summary>
        public double MinimumLength { get; }

        public WireInfo(Vector2 orientation, double length, double minimum)
        {
            Orientation = orientation;
            Length = length;
            MinimumLength = minimum;
        }
    }
}
