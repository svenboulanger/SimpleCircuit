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
        /// Gets or sets a flag indicating whether the wire is a bus.
        /// </summary>
        public bool IsBus { get; }

        /// <summary>
        /// Gets or sets a flag indicating whether the wire has a bus cross.
        /// </summary>
        public bool HasBusCross { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireInfo"/> struct.
        /// </summary>
        /// <param name="direction">The direction of the wire.</param>
        public WireInfo(char direction, bool isBus = false, bool hasBusCross = false)
        {
            Direction = direction;
            Length = -1.0;
            IsBus = isBus;
            HasBusCross = hasBusCross;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WireInfo"/> struct.
        /// </summary>
        /// <param name="direction">The direction of the wire.</param>
        /// <param name="length">The length of the wire.</param>
        public WireInfo(char direction, double length, bool isBus = false, bool hasBusCross = false)
        {
            Direction = direction;
            Length = length;
            IsBus = isBus;
            HasBusCross = hasBusCross;
        }
    }
}
