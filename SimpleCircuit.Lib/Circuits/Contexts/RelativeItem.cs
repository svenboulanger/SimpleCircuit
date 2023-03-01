namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An item that is relative to another item.
    /// </summary>
    public readonly struct RelativeItem
    {
        /// <summary>
        /// Gets the representative that this item is relative to.
        /// </summary>
        public string Representative { get; }

        /// <summary>
        /// Gets the offset compared to the representative item.
        /// </summary>
        public double Offset { get; }

        /// <summary>
        /// Creates a new <see cref="RelativeItem"/>.
        /// </summary>
        /// <param name="representative">The representative.</param>
        /// <param name="offset">The offset.</param>
        public RelativeItem(string representative, double offset)
        {
            Representative = representative;
            Offset = offset;
        }

        /// <summary>
        /// Converts the relative item to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Representative} + {Offset:G3}";
    }
}
