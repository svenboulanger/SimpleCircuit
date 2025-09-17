using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes a box shape that can be used for labeling.
    /// </summary>
    public interface IBoxDrawable : IDrawable
    {
        /// <summary>
        /// Gets the outer bounds.
        /// </summary>
        public Bounds OuterBounds { get; }

        /// <summary>
        /// Gets the inner bounds.
        /// </summary>
        public Bounds InnerBounds { get; }

        /// <summary>
        /// Gets the margins for labels when inside the box.
        /// </summary>
        public Margins InnerMargins { get; }

        /// <summary>
        /// Gets the margins for labels when outside the box.
        /// </summary>
        public double OuterMargin { get; }
    }
}
