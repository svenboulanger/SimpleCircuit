using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes the placement of a label.
    /// </summary>
    public struct LabelAnchorPoint
    {
        /// <summary>
        /// Class name for labels.
        /// </summary>
        public const string LabelClass = "lbl";

        /// <summary>
        /// Gets or sets the location of the label.
        /// </summary>
        public Vector2 Location { get; }

        /// <summary>
        /// Gets the label orientation.
        /// </summary>
        public TextOrientation Orientation { get; }

        /// <summary>
        /// Gets the label expansion (if <see cref="Orientation"/> is normal or vertical text).
        /// </summary>
        public Vector2 Expand { get; }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        public LabelAnchorPoint()
        {
            Location = default;
            Orientation = TextOrientation.Normal;
        }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The label orientation.</param>
        public LabelAnchorPoint(Vector2 location, TextOrientation orientation)
        {
            Location = location;
            Orientation = orientation;
        }
    }
}
