using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// An arrow marker.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    [Drawable("arrow", "An arrow", "General")]
    public class Arrow(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(-2.5, -1), new(0, 0), new(-2.5, 1)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle style)
        {
            builder.Polygon(_points.Select(pt => pt * 2.0 * style.LineThickness), style.AsFilledMarker());
        }
    }
}
