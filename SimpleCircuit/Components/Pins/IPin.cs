namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// Describes a pin of a component. This always at least
    /// has a location.
    /// </summary>
    public interface IPin : ILocatedPresence
    {
        /// <summary>
        /// Gets the pin description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets the number of items connected to the pin.
        /// </summary>
        public int Connections { get; set; }

        /// <summary>
        /// Gets the owner of the pin.
        /// </summary>
        public ILocatedDrawable Owner { get; }
    }
}
