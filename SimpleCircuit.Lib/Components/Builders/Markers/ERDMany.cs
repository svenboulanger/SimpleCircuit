using SimpleCircuit.Components.Appearance;
using System.Linq;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "many".
    /// </summary>
    /// <remarks>
    /// Creates a new entity-relationship diagram marker for "many".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ERDMany(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IAppearanceOptions appearance)
        {
            var options = appearance.CreateMarkerOptions();
            builder.Polyline(_points.Select(p => p * 2.0 * appearance.LineThickness), options);
            builder.Line(new Vector2(-4, 0) * appearance.LineThickness, new(), appearance);
        }
    }
}
