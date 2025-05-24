using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components.Builders
{
    /// <summary>
    /// An enumeration of possible text orientations.
    /// </summary>
    public readonly struct TextOrientation
    {
        /// <summary>
        /// Gets a normal text orientation.
        /// </summary>
        public static TextOrientation Normal { get; } = new TextOrientation(Vector2.UX, TextOrientationTypes.Normal);

        /// <summary>
        /// Gets a transformed text orientation.
        /// </summary>
        public static TextOrientation Transformed { get; } = new TextOrientation(Vector2.UX, TextOrientationTypes.Transformed);

        /// <summary>
        /// Gets the orientation of the text.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets whether the text should be transformed.
        /// </summary>
        public TextOrientationTypes Type { get; }

        /// <summary>
        /// Creates a new <see cref="TextOrientation"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <param name="type">If <c>true</c>, the text should be transformed along with whatever transform applies.</param>
        public TextOrientation(Vector2 orientation, TextOrientationTypes type)
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
            Type = TextOrientationTypes.Normal;
        }

        /// <summary>
        /// Transforms text bounds according to the text orientation and a given transform..
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>Returns the bounds of the transformed text.</returns>
        public Bounds TransformTextBounds(Bounds bounds, Transform transform)
        {
            switch (Type)
            {
                case TextOrientationTypes.Normal:
                    return bounds;

                case TextOrientationTypes.Transformed:
                    var b = new ExpandableBounds();
                    b.Expand(transform.ApplyDirection(new Vector2(bounds.Left, bounds.Bottom)));
                    b.Expand(transform.ApplyDirection(new Vector2(bounds.Left, bounds.Top)));
                    b.Expand(transform.ApplyDirection(new Vector2(bounds.Right, bounds.Top)));
                    b.Expand(transform.ApplyDirection(new Vector2(bounds.Right, bounds.Bottom)));
                    return b.Bounds;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
