namespace SimpleCircuit.Components
{
    /// <summary>
    /// A drawable whose visual style depends on the standard being used.
    /// </summary>
    public interface IStandardizedDrawable : IDrawable
    {
        /// <summary>
        /// Gets or sets the standard for which the component needs to be drawn.
        /// </summary>
        public Standards Supported { get; }
    }
}
