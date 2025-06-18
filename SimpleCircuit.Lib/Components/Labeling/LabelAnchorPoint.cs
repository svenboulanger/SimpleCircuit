using SimpleCircuit.Drawing.Builders;

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
        /// Gets the orientation.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets the label expansion (if <see cref="Orientation"/> is normal or vertical text).
        /// </summary>
        public Vector2 Expand { get; }

        /// <summary>
        /// Gets the text orientation type.
        /// </summary>
        public TextOrientationType Type { get; }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        public LabelAnchorPoint()
        {
            Location = default;
            Expand = default;
            Orientation = Vector2.UX;
            Type = TextOrientationType.Upright;
        }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion direction.</param>
        public LabelAnchorPoint(Vector2 location, Vector2 expand)
        {
            Location = location;
            Expand = expand;
            Orientation = Vector2.UX;
            Type = TextOrientationType.Upright;
        }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The label orientation.</param>
        public LabelAnchorPoint(Vector2 location, Vector2 expand, Vector2 orientation, TextOrientationType type)
        {
            Location = location;
            Expand = expand;
            Orientation = orientation;
            Type = type;
        }
    }
}
