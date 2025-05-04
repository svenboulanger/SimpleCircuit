using SimpleCircuit.Components.Appearance;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "only one".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "only one".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDOnlyOne(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IAppearanceOptions appearance)
        {
            builder.Line(new Vector2(-4, -3) * appearance.LineThickness, new Vector2(-4, 3) * appearance.LineThickness, appearance);
            builder.Line(new Vector2(-8, -3) * appearance.LineThickness, new Vector2(-8, 3) * appearance.LineThickness, appearance);
        }
    }
}
