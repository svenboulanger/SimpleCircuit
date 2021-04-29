namespace SimpleCircuit.Components
{
    public interface IPin : ITranslating
    {
        /// <summary>
        /// Gets the name of the pin.
        /// </summary>
        /// <value>
        /// The name of the pin.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the pin description.
        /// </summary>
        /// <value>
        /// The pin description.
        /// </value>
        string Description { get; }

        /// <summary>
        /// Gets the owner of the pin.
        /// </summary>
        /// <value>
        /// The pin owner.
        /// </value>
        IComponent Owner { get; }
    }
}
