using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Describes the placement of a label.
    /// </summary>
    public struct LabelAnchorPoint
    {
        /// <summary>
        /// Gets the default graphic options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new GraphicOptions(LabelClass);

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
        public GraphicOptions Options { get; }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        public LabelAnchorPoint()
        {
            Location = default;
            Expand = default;
            Options = DefaultOptions;
        }

        /// <summary>
        /// Creates a new <see cref="LabelAnchorPoint"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="expand">The expansion.</param>
        /// <param name="options">The graphic options.</param>
        public LabelAnchorPoint(Vector2 location, Vector2 expand, GraphicOptions options = null)
        {
            Location = location;
            Expand = expand;
            Options = options ?? DefaultOptions;
        }
    }
}
