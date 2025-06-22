using SimpleCircuit.Drawing;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// An enumeration of possible text orientations.
    /// </summary>
    public readonly struct TextOrientation
    {
        /// <summary>
        /// Gets a normal text orientation.
        /// </summary>
        public static TextOrientation Normal { get; } = new TextOrientation(Vector2.UX, TextOrientationType.Upright);

        /// <summary>
        /// Gets a vertical text orientation.
        /// </summary>
        public static TextOrientation Vertical { get; } = new TextOrientation(Vector2.UY, TextOrientationType.Upright);

        /// <summary>
        /// Gets a transformed text orientation.
        /// </summary>
        public static TextOrientation Transformed { get; } = new TextOrientation(Vector2.UX, TextOrientationType.Transformed | TextOrientationType.Upright);

        /// <summary>
        /// Gets the orientation of the text.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets whether the text should be transformed.
        /// </summary>
        public TextOrientationType Type { get; }

        /// <summary>
        /// Creates a new <see cref="TextOrientation"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <param name="type">If <c>true</c>, the text should be transformed along with whatever transform applies.</param>
        public TextOrientation(Vector2 orientation, TextOrientationType type)
        {
            Orientation = orientation;
            Type = type;
        }

        /// <summary>
        /// Creates a new <see cref="TextOrientation"/> that is upright but expands in a certain direction.
        /// </summary>
        /// <param name="x">The X-coordinate of the text orientation.</param>
        /// <param name="y">The Y-coordinate of the text orientation.</param>
        public TextOrientation(double x, double y)
        {
            Orientation = new Vector2(x, y);
            Type = TextOrientationType.Upright;
        }
    }
}
