namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// An instance that is labeled.
    /// </summary>
    public interface ILabeled
    {
        /// <summary>
        /// Gets the labels supporting the drawable.
        /// </summary>
        public Labels Labels { get; }
    }
}
