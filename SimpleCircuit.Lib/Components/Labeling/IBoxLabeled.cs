namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes a box shape that can be used for labeling.
    /// </summary>
    public interface IBoxLabeled : ILabeled
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
        /// Gets or sets the margin of labels from the edge.
        /// </summary>
        public double LabelMargin { get; set; }
    }
}
