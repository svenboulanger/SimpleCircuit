namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Describes a change in annotations.
    /// </summary>
    public abstract class AnnotationChange
    {
        /// <summary>
        /// Applies the change.
        /// </summary>
        /// <param name="context">The parsing context.</param>
        /// <returns>Returns <c>true</c> if the change was made; otherwise, <c>false</c>.</returns>
        public abstract bool Apply(ParsingContext context);
    }
}
