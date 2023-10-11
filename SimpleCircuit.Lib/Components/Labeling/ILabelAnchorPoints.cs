namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes label anchor points for a subject.
    /// </summary>
    /// <typeparam name="T">The subject type.</typeparam>
    public interface ILabelAnchorPoints<in T> where T : ILabeled
    {
        /// <summary>
        /// Gets the number of anchor points defined.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Calculates the anchor point at the given index for the subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="index">The index.</param>
        /// <returns>The label anchor point.</returns>
        public LabelAnchorPoint Calculate(T subject, int index);

        /// <summary>
        /// Draws labels using the current set of label anchor points.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="subject">The subject.</param>
        public void Draw(SvgDrawing drawing, T subject);
    }
}
