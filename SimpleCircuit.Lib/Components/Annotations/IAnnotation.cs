namespace SimpleCircuit.Components.Annotations
{
    /// <summary>
    /// Describes an annotation for other components.
    /// </summary>
    public interface IAnnotation : IDrawable
    {
        /// <summary>
        /// Adds a drawable to the annotation.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public void Add(IDrawable drawable);
    }
}
