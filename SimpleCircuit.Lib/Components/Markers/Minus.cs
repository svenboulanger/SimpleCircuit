using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// A minus marker.
    /// </summary>
    /// <remarks>
    /// Creates a new minus marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    [Drawable("minus", "A generic minus symbol.", "General")]
    public class Minus(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <summary>
        /// Gets whether the plus should be drawn on the opposite side.
        /// </summary>
        public bool OppositeSide { get; set; }

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            Vector2 offset = OppositeSide ? new(-2.5, 3) : new(-2.5, -3);
            builder.BeginTransform(new(offset, builder.CurrentTransform.Matrix.Inverse));
            builder.Line(new(-1, 0), new(1, 0), appearance);
            builder.EndTransform();
        }
    }
}
