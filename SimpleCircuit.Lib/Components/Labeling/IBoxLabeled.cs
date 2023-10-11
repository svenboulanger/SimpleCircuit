namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes a boxed item with a width and height.
    /// </summary>
    public interface IBoxLabeled
    {
        /// <summary>
        /// Gets the top-left coordinate of the box.
        /// </summary>
        public Vector2 TopLeft { get; }

        /// <summary>
        /// Gets the bottom-right coordinate of the box.
        /// </summary>
        public Vector2 BottomRight { get; }

        /// <summary>
        /// Gets the radius if the box has rounded corners.
        /// </summary>
        public double CornerRadius { get; }

        /// <summary>
        /// Gets the margin of labels from the edge.
        /// </summary>
        public double LabelMargin { get; }
    }
}
