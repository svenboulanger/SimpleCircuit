using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Drawing.Builders.Markers
{
    /// <summary>
    /// An entity-relation diagram marker for "zero or one".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "zero or one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDZeroOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Circle(new(-11 * appearance.LineThickness, 0), 3 * appearance.LineThickness, appearance);
            builder.Line(new Vector2(-4, -3) * appearance.LineThickness, new Vector2(-4, 3) * appearance.LineThickness, appearance);
        }
    }
}
