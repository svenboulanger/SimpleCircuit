using SimpleCircuit.Components.Appearance;

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
        /// Gets or sets the expansion direction of the label.
        /// </summary>
        public Vector2 Expand { get; }

        /// <summary>
        /// Gets the options used for the label.
        /// </summary>
        public IAppearanceOptions Appearance { get; }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        public LabelAnchorPoint()
        {
            Location = default;
            Expand = default;
            Appearance = new AppearanceOptions();
        }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion.</param>
        /// <param name="appearance">The appearance options.</param>
        public LabelAnchorPoint(Vector2 location, Vector2 expand, IAppearanceOptions appearance)
        {
            Location = location;
            Expand = expand;
            Appearance = appearance;
        }
    }
}
