using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Drawing.Builders.Markers
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
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Polyline(_points.Select(p => p * 2.0 * appearance.LineThickness), appearance);
            builder.Line(new Vector2(-4, 0) * appearance.LineThickness, new(), appearance);
        }
    }
}
