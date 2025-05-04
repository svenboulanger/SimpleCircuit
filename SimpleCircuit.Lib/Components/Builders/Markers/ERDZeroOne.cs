using SimpleCircuit.Components.Appearance;

namespace SimpleCircuit.Components.Builders.Markers
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
        protected override void DrawMarker(IGraphicsBuilder builder, IAppearanceOptions appearance)
        {
            builder.RequiredCSS.Add(".marker.erd.zero { fill: white; }");
            GraphicOptions options = appearance.CreateMarkerOptions();
            options.Style["fill"] = appearance.Background;
            builder.Circle(new(-11 * appearance.LineThickness, 0), 3 * appearance.LineThickness, options);
            builder.Line(new Vector2(-4, -3) * appearance.LineThickness, new Vector2(-4, 3) * appearance.LineThickness, appearance);
        }
    }
}
