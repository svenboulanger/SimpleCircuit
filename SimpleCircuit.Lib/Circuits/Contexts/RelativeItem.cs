namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An item that is relative to another item.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="RelativeItem"/>.
    /// </remarks>
    /// <param name="representative">The representative.</param>
    /// <param name="offset">The offset.</param>
    public readonly struct RelativeItem(string representative, double offset)
    {
        /// <summary>
        /// Gets the representative that this item is relative to.
        /// </summary>
        public string Representative { get; } = representative;

        /// <summary>
        /// Gets the offset compared to the representative item.
        /// </summary>
        public double Offset { get; } = offset;

        /// <summary>
        /// Converts the relative item to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Representative} + {Offset:G3}";
    }
}
