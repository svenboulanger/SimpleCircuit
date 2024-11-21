namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes a circular item with a center and radius.
    /// </summary>
    public interface IEllipseDrawable : IDrawable
    {
        /// <summary>
        /// Gets the center of the ellipse.
        /// </summary>
        public Vector2 Center { get; }

        /// <summary>
        /// Gets the radius of the ellipse along the x-axis.
        /// </summary>
        public double RadiusX { get; }

        /// <summary>
        /// Gets the radius of the ellipse along the y-axis.
        /// </summary>
        public double RadiusY { get; }

        /// <summary>
        /// Gets the margin of labels from the edge.
        /// </summary>
        public double LabelMargin { get; set; }
    }
}
