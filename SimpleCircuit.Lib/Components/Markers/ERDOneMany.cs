using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "one or many".
    /// </summary>
    /// <remarks>
    /// Creates an entity-relationship diagram marker for "one or many".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    [Drawable("erd-one-many", "An Entity-Relationship Diagram one or many-symbol.", "ERD")]
    public class ERDOneMany(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Polyline(_points, appearance);
            builder.Line(new(-3, -1.5), new(-3, 1.5), appearance);
        }
    }
}
