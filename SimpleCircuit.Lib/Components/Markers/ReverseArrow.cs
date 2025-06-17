using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// A reversed arrow marker.
    /// </summary>
    /// <remarks>
    /// Creates a new arrow marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    [Drawable("reverse-arrow", "A reverse arrow symbol.", "General")]
    public class ReverseArrow(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1), new(-2.5, 0), new(0, 1)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            builder.Polygon(_points.Select(pt => pt * 2.0 * appearance.LineThickness), appearance.AsFilledMarker());
        }
    }
}
