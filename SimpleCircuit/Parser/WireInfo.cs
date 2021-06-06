namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public struct WireInfo
    {
        /// <summary>
        /// Gets the direction of the wire.
        /// </summary>
        public char Direction { get; }

        /// <summary>
        /// Gets the length of the wire.
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireInfo"/> struct.
        /// </summary>
        /// <param name="direction">The direction of the wire.</param>
        public WireInfo(char direction)
        {
            Direction = direction;
            Length = -1.0;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WireInfo"/> struct.
        /// </summary>
        /// <param name="direction">The direction of the wire.</param>
        /// <param name="length">The length of the wire.</param>
        public WireInfo(char direction, double length)
        {
            Direction = direction;
            Length = length;
        }
    }
}
