using SimpleCircuit.Components.Styles;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one".
    /// </summary>
    /// <remarks>
    /// Creates a new entity-relationship diagram marker for "one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Line(new Vector2(-4, -3) * appearance.LineThickness, new Vector2(-4, 3) * appearance.LineThickness, appearance);
        }
    }
}
