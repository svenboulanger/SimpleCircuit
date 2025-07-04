using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes label anchor points for a subject.
    /// </summary>
    /// <typeparam name="T">The subject type.</typeparam>
    public interface ILabelAnchorPoints<in T> where T : IDrawable
    {
        /// <summary>
        /// Gets the number of anchor points defined.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Tries to assign an anchor index for a given anchor name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="index">The index.</param>
        /// <returns>Returns <c>true</c> if the index could be found; otherwise, <c>false</c>.</returns>
        public bool TryGetAnchorIndex(string name, out int index);

        /// <summary>
        /// Calculates the bounds of all labels placed at the given anchor index, assuming each label would be located at (0, 0).
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="index">The anchor index.</param>
        /// <param name="context">The text formatter that can be used to determine text format.</param>
        /// <returns>Returns the bounds.</returns>
        public Bounds GetBounds(T subject, int index, ITextFormatter context, IStyle style);

        /// <summary>
        /// Gets a <see cref="LabelAnchorPoint"/> for the given anchor index.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="index">The index.</param>
        /// <param name="style">The relevant style, which can be used to fine-tune anchor location.</param>
        /// <returns>Returns the label anchor point.</returns>
        public LabelAnchorPoint GetAnchorPoint(T subject, int index, IStyle style);

        /// <summary>
        /// Draws all labels of a <paramref name="subject"/> using the current set of label anchor points.
        /// </summary>
        /// <param name="builder">The graphics builder.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="parentStyle">The parent style.</param>
        public void Draw(IGraphicsBuilder builder, T subject, IStyle parentStyle);
    }
}
